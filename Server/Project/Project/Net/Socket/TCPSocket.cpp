#include "SocketHeader.h"

int TCPSocket::socket_connect(const SocketAddress& in_address)
{
	int err = connect(m_socket, &in_address.m_sock_addr, in_address.get_size());
	if (err < 0)
	{
		SocketMgr::report_error("TCPSocket::Connect");
		return -SocketMgr::get_last_error();
	}
	return NO_ERROR;
}

int TCPSocket::socket_listen(int in_back_log)
{
	int err = listen(m_socket, in_back_log);
	if (err < 0)
	{
		SocketMgr::report_error("TCPSocket::Listen");
		return -SocketMgr::get_last_error();
	}
	return NO_ERROR;
}

TCPSocketPtr TCPSocket::socket_accept(SocketAddress& in_from_address)
{
	socklen_t length = in_from_address.get_size();
	SOCKET newSocket = accept(m_socket, &in_from_address.m_sock_addr, &length);

	if (newSocket != INVALID_SOCKET)
	{
		return TCPSocketPtr(new TCPSocket(newSocket));
	}
	else
	{
		SocketMgr::report_error("TCPSocket::Accept");
		return nullptr;
	}
}

int32_t	TCPSocket::socket_send(const void* in_data, size_t in_len)
{
	int bytesSentCount = send(m_socket, static_cast<const char*>(in_data), in_len, 0);
	if (bytesSentCount < 0)
	{
		SocketMgr::report_error("TCPSocket::Send");
		return -SocketMgr::get_last_error();
	}
	return bytesSentCount;
}

int32_t	TCPSocket::socket_receive(void* in_data, size_t in_len)
{
	int bytesReceivedCount = recv(m_socket, static_cast<char*>(in_data), in_len, 0);
	if (bytesReceivedCount < 0)
	{
		SocketMgr::report_error("TCPSocket::Receive");
		return -SocketMgr::get_last_error();
	}
	return bytesReceivedCount;
}

int TCPSocket::socket_bind(const SocketAddress& inBindAddress)
{
	int error = bind(m_socket, &inBindAddress.m_sock_addr, inBindAddress.get_size());
	if (error != 0)
	{
		SocketMgr::report_error("TCPSocket::Bind");
		return SocketMgr::get_last_error();
	}

	return NO_ERROR;
}

TCPSocket::~TCPSocket()
{
#if _WIN32
	closesocket(m_socket);
#else
	close(m_socket);
#endif
}
