#pragma once
class UDPSocket
{
public:

	~UDPSocket();

	int bind_addr(const SocketAddress& inToAddress);
	int send_to(const void* inToSend, int inLength, const SocketAddress& inToAddress);
	int receive_from(void* inToReceive, int inMaxLength, SocketAddress& outFromAddress);

	/*
	int SendTo( const MemoryOutputStream& inMOS, const SocketAddress& inToAddress );
	int ReceiveFrom( MemoryInputStream& inMIS, SocketAddress& outFromAddress );
	*/

	int set_non_blocking_mode(bool inShouldBeNonBlocking);

private:
	friend class SocketMgr;
	UDPSocket(SOCKET in_socket) : m_socket(in_socket) {}
	SOCKET m_socket;

};

typedef shared_ptr< UDPSocket >	UDPSocketPtr;
