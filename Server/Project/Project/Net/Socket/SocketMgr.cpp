#include "SocketHeader.h"

bool SocketMgr::init()
{
#if _WIN32
	WSADATA wsa_data;
	int res = WSAStartup(MAKEWORD(2, 2), &wsa_data);
	if (res != NO_ERROR)
	{
		report_error("Starting Up");
		return false;
	}
#endif
	return true;
}

void SocketMgr::cleanup()
{
	WSACleanup();
}


void SocketMgr::report_error(const char* operation_desc)
{
#if _WIN32
	LPVOID lp_msg_buf;
	DWORD error_num = get_last_error();

	FormatMessage(
		FORMAT_MESSAGE_ALLOCATE_BUFFER |
		FORMAT_MESSAGE_FROM_SYSTEM |
		FORMAT_MESSAGE_IGNORE_INSERTS,
		NULL,
		error_num,
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
		(LPTSTR)&lp_msg_buf,
		0, NULL);

	LOG("Error %s: %d- %s", operation_desc, error_num, lp_msg_buf);
#else
	LOG("Error: %hs", operation_desc);
#endif
}

int SocketMgr::get_last_error()
{
#if _WIN32
	return WSAGetLastError();
#else
	return errno;
#endif
}

TCPSocketPtr SocketMgr::create_tcp_socket(ESocketAddressFamily family)
{
	SOCKET s = socket(family, SOCK_STREAM, IPPROTO_TCP);

	if (s != INVALID_SOCKET)
	{
		return TCPSocketPtr(new TCPSocket(s));
	}
	else
	{
		report_error("SocketMgr::CreateTCPSocket");
		return nullptr;
	}
}

fd_set* SocketMgr::fill_set_from_vector(fd_set& out_set, const vector< TCPSocketPtr >* in_sockets, int& io_nax_nfds)
{
	if (in_sockets)
	{
		FD_ZERO(&out_set);

		for (const TCPSocketPtr& socket : *in_sockets)
		{
			FD_SET(socket->m_socket, &out_set);
#if !_WIN32
			io_nax_nfds = std::max(io_nax_nfds, socket->m_socket);
#endif
		}
		return &out_set;
	}
	else
	{
		return nullptr;
	}
}

void SocketMgr::fill_vector_from_set(vector< TCPSocketPtr >* out_sockets, const vector< TCPSocketPtr >* in_sockets, const fd_set& in_set)
{
	if (in_sockets && out_sockets)
	{
		out_sockets->clear();
		for (const TCPSocketPtr& socket : *in_sockets)
		{
			if (FD_ISSET(socket->m_socket, &in_set))
			{
				out_sockets->push_back(socket);
			}
		}
	}
}

int SocketMgr::socket_select(const vector< TCPSocketPtr >* in_read_set,
	vector< TCPSocketPtr >* out_read_set,
	const vector< TCPSocketPtr >* in_write_set,
	vector< TCPSocketPtr >* out_write_set,
	const vector< TCPSocketPtr >* in_except_set,
	vector< TCPSocketPtr >* out_except_set)
{
	//build up some sets from our vectors
	fd_set read, write, except;

	int nfds = 0;

	fd_set *readPtr = fill_set_from_vector(read, in_read_set, nfds);
	fd_set *writePtr = fill_set_from_vector(write, in_write_set, nfds);
	fd_set *exceptPtr = fill_set_from_vector(except, in_except_set, nfds);

	int ret = select(nfds + 1, readPtr, writePtr, exceptPtr, nullptr);

	if (ret > 0)
	{
		fill_vector_from_set(out_read_set, in_read_set, read);
		fill_vector_from_set(out_write_set, in_write_set, write);
		fill_vector_from_set(out_except_set, in_except_set, except);
	}
	return ret;
}

