#pragma once

class Engine {
public:
    //单例
    static Engine* inst;
public:
    //构造函数
    Engine();
    //初始化并开始
    void Start();
    //等待运行
    void Wait();
    //增删服务
    uint32_t NewService(shared_ptr<string> type);
    void KillService(uint32_t id);     //仅限服务自己调用
    //发送消息
    void Send(uint32_t toId, shared_ptr<BaseMsg> msg);
    //全局队列操作
    shared_ptr<Service> PopGlobalQueue();
    void PushGlobalQueue(shared_ptr<Service> srv);
    //让工作线程等待（仅工作线程调用）
    void WorkerWait();
    //仅测试
    shared_ptr<BaseMsg> MakeMsg(uint32_t source, char* buff, int len);
    //增删查Conn
    int AddConn(TCPSocketPtr in_socket, uint32_t id, Conn::TYPE type);
    shared_ptr<Conn> GetConn(int fd);
    bool RemoveConn(int fd);
    //网络连接操作接口（用原始read write）
    int Listen(uint32_t port, uint32_t serviceId);
    void CloseConn(uint32_t fd);
    //对外Event接口
    void ModifyEvent(int fd, bool epollOut);

    shared_ptr<Service> GetService(uint32_t id);
    unordered_map<uint32_t, shared_ptr<Service>> services;

private:
    //工作线程
    int WORKER_NUM = 3;              //工作线程数（配置）
    vector<Worker*> workers;         //worker对象
    vector<thread*> workerThreads;   //线程
    //Socket线程
    SocketWorker* socketWorker;

    thread* socketThread;
    //服务列表
    uint32_t maxId = 0;              //最大ID
    RALL_RWLOCK services_lock ; //读写锁
    //全局队列
    queue<shared_ptr<Service>> globalQueue;
    int globalLen = 0;               //队列长度
    SpinLock globalLock;   //锁
    //休眠和唤醒
    mutex sleepMtx;
    condition_variable sleepCond;
    int sleepCount = 0;        //休眠工作线程数 
    //Conn列表
    unordered_map<uint32_t, shared_ptr<Conn>> conns;
    RALL_RWLOCK conns_lock;//读写锁 connsLock;   //读写锁

private:
    //开启工作线程
    void StartWorker();
    //唤醒工作线程
    void CheckAndWeakUp();
    //开启Socket线程
    void StartSocket();
    //获取服务
};