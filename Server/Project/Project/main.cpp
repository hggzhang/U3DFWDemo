#include "NetHeader.h"
char recvBuffer[4096];

int test1()
{
	SocketMgr::init();
	TCPSocketPtr socketListener = SocketMgr::create_tcp_socket(ESocketAddressFamily::INET);
	if (socketListener == nullptr) return -1;

	SocketAddressPtr socketAddressPtr = SocketAddressPtr(new SocketAddress(INADDR_ANY, 8888));
	SocketAddress ss = *socketAddressPtr;

	if (socketListener->socket_bind(*socketAddressPtr)) return -1;
	if (socketListener->socket_listen(5) == SOCKET_ERROR) return -1;

	printf("------------服务器监听已启动-------------\n");
	SocketAddress* socketAddress = new SocketAddress();

	while (true)
	{
		TCPSocketPtr clientSocket = socketListener->socket_accept(*socketAddressPtr);
		if (clientSocket)
		{
			printf("----------已建立连接------------\n");
			while (true)
			{
				int ret = clientSocket->socket_receive(recvBuffer, 255);
				if (ret > 0)
				{
					recvBuffer[ret] = 0x00;
					printf("----------receive data------------\n");
					printf(recvBuffer);
					printf("\n");
				}
				if (clientSocket == nullptr) break;
			}
		}
	}
	SocketMgr::cleanup();
}



int main()
{
	new Engine();
	if (!SocketMgr::init()) return -1;
	Engine::inst->Start();
	auto t = make_shared<string>("main");
	int sid = Engine::inst->NewService(t);
	Engine::inst->Wait();
	
	return 0;
}