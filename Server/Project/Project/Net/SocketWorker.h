#pragma once

class SocketWorker { 
public:
    SocketWorker();
    ~SocketWorker();
    void Init();        //初始化
    void operator()();  //线程函数
    int test = 0;
public:
    void add_socket(TCPSocketPtr in);
    void remove_socket(TCPSocketPtr in);
    void add_write_socket(TCPSocketPtr in);
    void remove_write_socket(TCPSocketPtr in);
private:
    void OnAccept(shared_ptr<Conn> conn);
    void OnRW(shared_ptr<Conn> conn, bool r, bool w);
    vector<TCPSocketPtr>* sockets;
    vector<TCPSocketPtr>* in_select_write_sockets;
    vector<TCPSocketPtr>* out_select_read_sockets;
    vector<TCPSocketPtr>* out_select_write_sockets;
    vector<TCPSocketPtr>* out_select_excpt_sockets;
};