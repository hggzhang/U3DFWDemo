#include "NetHeader.h"

//那些调Engine的通过传参数解决
//状态是不在队列中，global=true
void Worker::CheckAndPutGlobal(shared_ptr<Service> srv) {
    //退出中（只能自己调退出，isExiting不会线程冲突）
    if(srv->isExiting){ 
        return; 
    }

    srv->queueLock.lock();
    {
        //重新放回全局队列
        if(!srv->msgQueue.empty()) {
            //此时srv->inGlobal一定是true
            Engine::inst->PushGlobalQueue(srv);
        }
        //不在队列中，重设inGlobal
        else {
            srv->SetInGlobal(false);
        }
    }
    srv->queueLock.unlock();
}



//线程函数
void Worker::operator()() {
    while(true) {
        shared_ptr<Service> srv = Engine::inst->PopGlobalQueue();
        if (!srv)
        {
            for (auto s : Engine::inst->services)
            {
                auto sc = s.second;
                if (!sc->msgQueue.empty())
                    srv = sc;
            }
        }

        if(!srv){
            Engine::inst->WorkerWait();
        }
        else{
            srv->ProcessMsgs(eachNum);
            CheckAndPutGlobal(srv);
        }
    }
}