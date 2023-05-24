#include "NetHeader.h"

using namespace std;

//单例
Engine* Engine::inst;
Engine::Engine(){
    inst = this;
}

//开启worker线程
void Engine::StartWorker() {
    for (int i = 0; i < WORKER_NUM; i++) {
        cout << "start worker thread:" << i << endl;
        //创建线程对象
        Worker* worker = new Worker();
        worker->id = i;
        worker->eachNum = 2 << i;
        //创建线程
        thread* wt = new thread(*worker);
        //添加到列表
        workers.push_back(worker);
        workerThreads.push_back(wt);
    }
}

//开启Socket线程
void Engine::StartSocket() {
    //创建线程对象
    socketWorker = new SocketWorker();
    //初始化
    socketWorker->Init();
    //创建线程
    socketThread = new thread(*socketWorker);
}

//开启系统
void Engine::Start() {
    cout << "Hello Engine" << endl;
    StartWorker();
    //开启Socket线程
    StartSocket();
}

//等待
void Engine::Wait() {
    if( workerThreads[0]) {
        workerThreads[0]->join();
    }
}

//新建服务
uint32_t Engine::NewService(shared_ptr<string> type) {
    auto srv = make_shared<Service>();
    srv->type = type;
    {
        RALL_RWLOCK_WRITE_LOCK(services_lock);
        srv->id = maxId; 
        maxId++;
        services.emplace(srv->id, srv);
    }

    srv->OnInit(); //初始化
    return srv->id;
}

//由id查找服务
shared_ptr<Service> Engine::GetService(uint32_t id) {
    shared_ptr<Service> srv = NULL;

    {
        RALL_RWLOCK_READ_LOCK(services_lock);
        unordered_map<uint32_t, shared_ptr<Service>>::iterator iter = services.find (id);
        if (iter != services.end()){
            srv = iter->second;
        }
    }
    return srv;
}

//删除服务
//只能service自己调自己，因为srv->OnExit、srv->isExiting不加锁
void Engine::KillService(uint32_t id) {
    shared_ptr<Service> srv = GetService(id);
    if(!srv){
        return;
    }
    //退出前
    srv->OnExit();
    srv->isExiting = true;
    //删列表
    {
        RALL_RWLOCK_WRITE_LOCK(services_lock);
        services.erase(id);
    }
}


//发送消息
void Engine::Send(uint32_t toId, shared_ptr<BaseMsg> msg){
    shared_ptr<Service> toSrv = GetService(toId);
    if(!toSrv){
        cout << "Send fail, toSrv not exist toId:" << toId << endl;
        return;
    }
    toSrv->PushMsg(msg);
    //检查并放入全局队列
    //为缩小临界区灵活控制，破坏封装性
    bool hasPush = false;
    toSrv->inGlobalLock.lock();
    {
        if(!toSrv->inGlobal) {
            PushGlobalQueue(toSrv);
            toSrv->inGlobal = true;
            hasPush = true;
        }
    }
    toSrv->inGlobalLock.unlock();

    //唤起进程，不放在临界区里面
    if(hasPush) {
        CheckAndWeakUp();
    }
}

//弹出全局队列
shared_ptr<Service> Engine::PopGlobalQueue(){
    shared_ptr<Service> srv = NULL;
    globalLock.lock();
    {
        if (!globalQueue.empty()) {
            srv = globalQueue.front();
            globalQueue.pop();
            globalLen--;
        }
    }
    globalLock.unlock();
    return srv;
}

//插入全局队列
void Engine::PushGlobalQueue(shared_ptr<Service> srv){
    globalLock.lock();
    {
        globalQueue.push(srv);
        globalLen++;
    }
    globalLock.unlock();
}


//仅测试用，buff须由new产生
shared_ptr<BaseMsg> Engine::MakeMsg(uint32_t source, char* buff, int len) {
    auto msg= make_shared<ServiceMsg>();
    msg->type = BaseMsg::TYPE::SERVICE;
    msg->source = source;
    //基本类型的对象没有析构函数
    //所以回收基本类型组成的数组空间用delete 和 delete[]都可以
    //无需重新析构方法
    msg->buff = shared_ptr<char>(buff);
    msg->size = len;
    return msg;
}

//Worker线程调用，进入休眠
void Engine::WorkerWait(){
    sleepCount++;
    std::unique_lock<mutex> u_lock(sleepMtx);
    sleepCond.wait(u_lock);
    sleepCount--;
}


//检查并唤醒线程
void Engine::CheckAndWeakUp(){
    //unsafe
    if(sleepCount == 0) {
        return;
    }
    if( WORKER_NUM - sleepCount <= globalLen ) {
        cout << "weakup" << endl; 
        sleepCond.notify_one();
    }
}


//添加连接
int Engine::AddConn(TCPSocketPtr in_socket, uint32_t id, Conn::TYPE type) {
    auto conn = make_shared<Conn>();
    int fd = in_socket->m_socket;
    conn->m_socket = in_socket;
    conn->fd = fd;
    conn->serviceId = id;
    conn->type = type;
    {
        RALL_RWLOCK_WRITE_LOCK(conns_lock);
        conns.emplace(fd, conn);
    }
    return fd;
}

//由id查找连接
shared_ptr<Conn> Engine::GetConn(int fd) {
    shared_ptr<Conn> conn = NULL;
    {
        RALL_RWLOCK_READ_LOCK(conns_lock);
        unordered_map<uint32_t, shared_ptr<Conn>>::iterator iter = conns.find (fd);
        if (iter != conns.end()){
            conn = iter->second;
        }
    }
    return conn;
}

//删除连接
bool Engine::RemoveConn(int fd) {
    int result;
    {
        RALL_RWLOCK_WRITE_LOCK(conns_lock);
        result = conns.erase(fd);
    }
    return result == 1;
}

int Engine::Listen(uint32_t port, uint32_t serviceId) {

    auto listen_socket = SocketMgr::create_tcp_socket(INET);
    SocketAddress recv_addr(INADDR_ANY, port);
    if (listen_socket->socket_bind(recv_addr) != NO_ERROR)
    {
        return -1;
    }

    if (listen_socket->socket_listen(64) != NOERROR)
    {
        return -1;
    }

    AddConn(listen_socket, serviceId, Conn::TYPE::LISTEN);
    socketWorker->add_socket(listen_socket);
    return listen_socket->m_socket;
}


void Engine::CloseConn(uint32_t fd) {
    
    // TCPSocket析构函数会 close(socket)
    auto in_socket = GetConn(fd)->m_socket;
    bool succ = RemoveConn(fd);

    if(succ) {
        socketWorker->remove_socket(in_socket);
    }
}

void Engine::ModifyEvent(int fd, bool epollOut) {
    //socketWorker->ModifyEvent(fd, epollOut);
}