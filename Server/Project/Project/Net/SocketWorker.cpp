#include "NetHeader.h"


SocketWorker::SocketWorker()
{
    sockets = new vector<TCPSocketPtr>();
    out_select_read_sockets = new vector<TCPSocketPtr>();
    out_select_write_sockets = new vector<TCPSocketPtr>();
    out_select_excpt_sockets = new vector<TCPSocketPtr>();
    in_select_write_sockets = new vector<TCPSocketPtr>();

}

SocketWorker::~SocketWorker()
{
    delete sockets;
    delete out_select_read_sockets;
    delete out_select_write_sockets;
    delete out_select_excpt_sockets;
    delete in_select_write_sockets;

}

//初始化
void SocketWorker::Init() {
    cout << "SocketWorker Init" << endl;
}

void SocketWorker::OnAccept(shared_ptr<Conn> conn) {
    SocketAddress clent_addr;
    auto client = conn->m_socket->socket_accept(clent_addr);

    Engine::inst->AddConn(client, conn->serviceId, Conn::TYPE::CLIENT);
    add_socket(client);
    auto msg = make_shared<SocketAcceptMsg>();
    msg->type = BaseMsg::TYPE::SOCKET_ACCEPT;
    msg->listenFd = conn->fd;
    msg->clientFd = client->m_socket;
    Engine::inst->Send(conn->serviceId, msg);
}

void SocketWorker::OnRW(shared_ptr<Conn> conn, bool r, bool w) {
    cout << "OnRW fd:" << conn->fd << endl;
    auto msg= make_shared<SocketRWMsg>();
    msg->type = BaseMsg::TYPE::SOCKET_RW;
    msg->fd = conn->fd;
    msg->isRead = r;
    msg->isWrite = w;
    Engine::inst->Send(conn->serviceId, msg);
}

void SocketWorker::operator()() {
    while(true) {
        int n = SocketMgr::socket_select(sockets, out_select_read_sockets, in_select_write_sockets,
            out_select_write_sockets, sockets, out_select_excpt_sockets);
        if (n > 0)
        {
            struct socket_info
            {
                int fd;
                bool is_read = false;
                bool is_write = false;
                bool is_except = false;
            };

            map<int, socket_info> dict;
            for (auto& v : *out_select_read_sockets)
            {
                auto fd = v->m_socket;
                auto it = dict.find(fd);
                if (it == dict.end())
                {
                    dict[fd] = socket_info();
                    dict[fd].fd = fd;
                }
                dict[fd].is_read = true;
            }

            for (auto& v : *out_select_write_sockets)
            {
                auto fd = v->m_socket;
                auto it = dict.find(fd);
                if (it == dict.end())
                {
                    dict[fd] = socket_info();
                    dict[fd].fd = fd;
                }
                dict[fd].is_write = true;
            }

            for (auto& v : *out_select_excpt_sockets)
            {
                auto fd = v->m_socket;
                auto it = dict.find(fd);
                if (it == dict.end())
                {
                    dict[fd] = socket_info();
                    dict[fd].fd = fd;
                }
                dict[fd].is_except = true;
            }

            for (auto& p : dict)
            {
                auto v = p.second;
                auto conn = Engine::inst->GetConn(v.fd);

                if (conn->type == Conn::TYPE::LISTEN) {
                    if (v.is_read) {
                        OnAccept(conn);
                    }
                }
                //普通Socket
                else {
                    if (v.is_read || v.is_write) {
                        OnRW(conn, v.is_read, v.is_write);
                    }
                    if (v.is_except) {
                        cout << "OnError fd:" << conn->fd << endl;
                    }
                }
            }
        }
    }
}

void SocketWorker::add_socket(TCPSocketPtr in)
{
    sockets->push_back(in);
}

void SocketWorker::remove_socket(TCPSocketPtr in)
{
    auto i = std::find(sockets->begin(), sockets->end(), in);
    sockets->erase(i);
}

void SocketWorker::add_write_socket(TCPSocketPtr in)
{
    in_select_write_sockets->push_back(in);

}

void SocketWorker::remove_write_socket(TCPSocketPtr in)
{
    auto i = std::find(in_select_write_sockets->begin(), in_select_write_sockets->end(), in);
    in_select_write_sockets->erase(i);
}
