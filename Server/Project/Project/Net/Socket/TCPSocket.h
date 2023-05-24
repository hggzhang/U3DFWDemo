#pragma once

class TCPSocket
{
public:
	~TCPSocket();
	int								socket_connect(const SocketAddress& in_address);
	int								socket_bind(const SocketAddress& in_to_address);
	int								socket_listen(int in_back_log = 32);
	shared_ptr< TCPSocket >			socket_accept(SocketAddress& in_from_address);
	int32_t							socket_send(const void* in_data, size_t in_len);
	int32_t							socket_receive(void* in_buffer, size_t in_len);
	SOCKET		m_socket;
private:
	friend class SocketMgr;
	TCPSocket(SOCKET in_socket) : m_socket(in_socket) {}
};
typedef shared_ptr< TCPSocket > TCPSocketPtr;
