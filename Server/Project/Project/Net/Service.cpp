#include "NetHeader.h"

//构造函数
Service::Service() {
    
}

//析构函数
Service::~Service(){
    
}

//插入消息
void Service::PushMsg(shared_ptr<BaseMsg> msg) {
    queueLock.lock();
    {
        msgQueue.push(msg);
    }
    queueLock.unlock();
}

//取出消息
shared_ptr<BaseMsg> Service::PopMsg() {
    shared_ptr<BaseMsg> msg = NULL;
    //取一条消息
    queueLock.lock();
    {
        if (!msgQueue.empty()) { 
            msg =  msgQueue.front();
            msgQueue.pop();
        }
    }
    queueLock.unlock();
    return msg;
}

//处理一条消息，返回值代表是否处理
bool Service::ProcessMsg() {
    shared_ptr<BaseMsg> msg = PopMsg();
    if(msg) {
        OnMsg(msg);
        return true;
    }
    else {
        return false;
    }
} 

//处理N条消息，返回值代表是否处理
void Service::ProcessMsgs(int max) {
    for(int i=0; i<max; i++){
        bool succ = ProcessMsg();
        if(!succ){
            break;
        }
    }
}

//创建服务后触发
void Service::OnInit() {
    cout << "[" << id <<"] OnInit"  << endl;
    //新建Lua虚拟机
    luaState = luaL_newstate();
    luaL_openlibs(luaState); 
    //注册Engine系统API
    LuaAPI::Register(luaState);
    //执行Lua文件

    if (luaL_dofile(luaState, "../service/config.lua") == 1) {
        cout << "run config fail:" << lua_tostring(luaState, -1) << endl;
    }

    string filename = "../service/" + *type + "/init.lua";
    int isok = luaL_dofile(luaState, filename.data());
    if(isok == 1){ //成功返回值为0，失败则为1.
         cout << "run lua fail:" << lua_tostring(luaState, -1) << endl;
    }
    //调用Lua函数
    lua_getglobal(luaState, "OnInit"); 
    lua_pushinteger(luaState, id); 
    isok = lua_pcall(luaState, 1, 0, 0);
    if(isok != 0){ //成功返回值为0，否则代表失败.
         cout << "call lua OnInit fail " << lua_tostring(luaState, -1) << endl;
    }
}

//收到客户端数据
void Service::OnSocketData(int fd, const char* buff, int len) {
    cout << "recv buff = " << buff << " len = " << len << endl;

    //调用Lua函数
    lua_getglobal(luaState, "OnSocketData"); 
    lua_pushinteger(luaState, fd); 
    lua_pushlstring(luaState, buff,len); 
    int isok = lua_pcall(luaState, 2, 0, 0);
    if(isok != 0){ //成功返回值为0，否则代表失败.
         cout << "call lua OnSocketData fail " << lua_tostring(luaState, -1) << endl;
    }
}

//套接字可写
void Service::OnSocketWritable(int fd) {
    cout << "OnSocketWritable " << fd << endl;
    auto w = writers[fd];
    w->OnWriteable();
}

//关闭连接前
void Service::OnSocketClose(int fd) {
    writers.erase(fd);
    cout << "OnSocketClose " << fd << endl;

    //调用Lua函数
    lua_getglobal(luaState, "OnSocketClose"); 
    lua_pushinteger(luaState, fd); 
    int isok = lua_pcall(luaState, 1, 0, 0);
    if(isok != 0){ //成功返回值为0，否则代表失败.
         cout << "call lua OnSocketClose fail " << lua_tostring(luaState, -1) << endl;
    }
}

//收到其他服务发来的消息
void Service::OnServiceMsg(shared_ptr<ServiceMsg> msg) {
    //调用Lua函数
    lua_getglobal(luaState, "OnServiceMsg"); 
    lua_pushinteger(luaState, msg->source); 
    lua_pushlstring(luaState, msg->buff.get(), msg->size); 
    int isok = lua_pcall(luaState, 2, 0, 0);
    if(isok != 0){ //成功返回值为0，否则代表失败.
         cout << "call lua OnServiceMsg fail " << lua_tostring(luaState, -1) << endl;
    }
}

//新连接
void Service::OnAcceptMsg(shared_ptr<SocketAcceptMsg> msg) {
    cout << "OnAcceptMsg " << msg->clientFd << endl;
    auto w = make_shared<ConnWriter>();
    w->fd = msg->clientFd;
    writers.emplace(msg->clientFd, w);

    //调用Lua函数
    lua_getglobal(luaState, "OnAcceptMsg"); 
    lua_pushinteger(luaState, msg->listenFd); 
    lua_pushinteger(luaState, msg->clientFd); 
    int isok = lua_pcall(luaState, 2, 0, 0);
    if(isok != 0){ //成功返回值为0，否则代表失败.
         cout << "call lua OnAcceptMsg fail " << lua_tostring(luaState, -1) << endl;
    }
}

//套接字可读可写
void Service::OnRWMsg(shared_ptr<SocketRWMsg> msg) {
    int fd = msg->fd;
    //可读
    if(msg->isRead) {
        const int BUFFSIZE = 512;
        char buff[BUFFSIZE];
        int len = 0;
        auto conn = Engine::inst->GetConn(fd);
        auto in_socket = conn->m_socket;
        do {
            //len = read(fd, &buff, BUFFSIZE);
            len = in_socket->socket_receive(&buff, BUFFSIZE);
            if(len > 0){
                OnSocketData(fd, buff, len);
            }
        }while(len == BUFFSIZE);

        if(len <= 0 && errno != EAGAIN) {
            if(Engine::inst->GetConn(fd)) {
                OnSocketClose(fd);
                Engine::inst->CloseConn(fd);
            }
        }
    }
    //可写（注意没有else）
    if(msg->isWrite) {
        if(Engine::inst->GetConn(fd)){
            OnSocketWritable(fd);
        }
    }
}



//收到消息时触发
void Service::OnMsg(shared_ptr<BaseMsg> msg) {
    //SERVICE
    if(msg->type == BaseMsg::TYPE::SERVICE) {
        auto m = dynamic_pointer_cast<ServiceMsg>(msg);
        OnServiceMsg(m);
    }
    //SOCKET_ACCEPT
    else if(msg->type == BaseMsg::TYPE::SOCKET_ACCEPT) {
        auto m = dynamic_pointer_cast<SocketAcceptMsg>(msg);
        OnAcceptMsg(m);
    }
    //SOCKET_RW
    else if(msg->type == BaseMsg::TYPE::SOCKET_RW) {
        auto m = dynamic_pointer_cast<SocketRWMsg>(msg);
        OnRWMsg(m);
    }
}


//退出服务时触发
void Service::OnExit() {
    cout << "[" << id <<"] OnExit"  << endl;
    //调用Lua函数
    lua_getglobal(luaState, "OnExit"); 
    int isok = lua_pcall(luaState, 0, 0, 0);
    if(isok != 0){ //成功返回值为0，否则代表失败.
         cout << "call lua OnExit fail " << 
            lua_tostring(luaState, -1) << endl;
    }
    //关闭lua虚拟机
    lua_close(luaState);
}

void Service::SetInGlobal(bool isIn) {
    inGlobalLock.lock();
    {
        inGlobal = isIn;
    }
    inGlobalLock.unlock();
}