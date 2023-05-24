#pragma once

enum ESocketAddressFamily
{
	INET = AF_INET,
	INET6 = AF_INET6
};

class SocketMgr
{
public:
	static bool	init();
	static void	cleanup();

	static void	report_error(const char* operation_desc);
	static int	get_last_error();

	static int	socket_select(const vector< TCPSocketPtr >* inReadSet,
	vector< TCPSocketPtr >* out_read_set,
	const vector< TCPSocketPtr >* in_write_set,
	vector< TCPSocketPtr >* out_write_set,
	const vector< TCPSocketPtr >* in_except_set,
	vector< TCPSocketPtr >* out_except_set);

	static TCPSocketPtr	create_tcp_socket(ESocketAddressFamily family);

private:
	inline static fd_set* fill_set_from_vector(fd_set& out_set, const vector< TCPSocketPtr >* in_sockets, int& io_nax_nfds);
	inline static void fill_vector_from_set(vector< TCPSocketPtr >* out_sockets, const vector< TCPSocketPtr >* in_sockets, const fd_set& in_set);
};

