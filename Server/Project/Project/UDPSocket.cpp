#include "SocketHeader.h"

int UDPSocket::bind_addr(const SocketAddress& inBindAddress)
{
	int error = bind(m_socket, &inBindAddress.m_sock_addr, inBindAddress.get_size());
	if (error != 0)
	{
		SocketMgr::report_error("UDPSocket::bind");
		return SocketMgr::get_last_error();
	}

	return NO_ERROR;
}

int UDPSocket::send_to(const void* inToSend, int inLength, const SocketAddress& inToAddress)
{
	int byteSentCount = sendto(m_socket,
		static_cast<const char*>(inToSend),
		inLength,
		0, &inToAddress.m_sock_addr, inToAddress.get_size());
	if (byteSentCount <= 0)
	{
		//we'll return error as negative number to indicate less than requested amount of bytes sent...
		SocketMgr::report_error("UDPSocket::send_to");
		return -SocketMgr::get_last_error();
	}
	else
	{
		return byteSentCount;
	}
}

int UDPSocket::receive_from(void* inToReceive, int inMaxLength, SocketAddress& outFromAddress)
{
	socklen_t fromLength = outFromAddress.get_size();

	int readByteCount = recvfrom(m_socket,
		static_cast<char*>(inToReceive),
		inMaxLength,
		0, &outFromAddress.m_sock_addr, &fromLength);
	if (readByteCount >= 0)
	{
		return readByteCount;
	}
	else
	{
		int error = SocketMgr::get_last_error();

		if (error == WSAEWOULDBLOCK)
		{
			return 0;
		}
		else if (error == WSAECONNRESET)
		{
			//this can happen if a client closed and we haven't DC'd yet.
			//this is the ICMP message being sent back saying the port on that computer is closed
			LOG("Connection reset from %s", outFromAddress.to_string().c_str());
			return -WSAECONNRESET;
		}
		else
		{
			SocketMgr::report_error("UDPSocket::receive_from");
			return -error;
		}
	}
}

UDPSocket::~UDPSocket()
{
#if _WIN32
	closesocket(m_socket);
#else
	close(m_socket);
#endif
}


int UDPSocket::set_non_blocking_mode(bool inShouldBeNonBlocking)
{
#if _WIN32
	u_long arg = inShouldBeNonBlocking ? 1 : 0;
	int result = ioctlsocket(m_socket, FIONBIO, &arg);
#else
	int flags = fcntl(m_socket, F_GETFL, 0);
	flags = inShouldBeNonBlocking ? (flags | O_NONBLOCK) : (flags & ~O_NONBLOCK);
	int result = fcntl(m_socket, F_SETFL, flags);
#endif

	if (result == SOCKET_ERROR)
	{
		SocketMgr::report_error("UDPSocket::set_non_blocking_mode");
		return SocketMgr::get_last_error();
	}
	else
	{
		return NO_ERROR;
	}
}

