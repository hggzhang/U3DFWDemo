#pragma once

class SocketAddress
{
public:
	SocketAddress(uint32_t in_address,uint16_t inPort)
	{
		get_as_sock_addr_in()->sin_family = AF_INET;
		get_ip4_ref() = htonl(in_address);
		get_as_sock_addr_in()->sin_port = htons(inPort);
	}

	SocketAddress(const sockaddr& inSockAddr)
	{
		memcpy(&m_sock_addr, &inSockAddr, sizeof(sockaddr));
	}

	SocketAddress()
	{
		get_as_sock_addr_in()->sin_family = AF_INET;
		get_ip4_ref() = INADDR_ANY;
		get_as_sock_addr_in()->sin_port = 0;
	}

	bool operator==(const SocketAddress& inOther) const
	{
		return (m_sock_addr.sa_family == AF_INET &&
			get_as_sock_addr_in()->sin_port == inOther.get_as_sock_addr_in()->sin_port) &&
			(get_ip4_ref() == inOther.get_ip4_ref());
	}

	size_t get_hash() const
	{
		return (get_ip4_ref()) |
			((static_cast<uint32_t>(get_as_sock_addr_in()->sin_port)) << 13) |
			m_sock_addr.sa_family;
	}

	uint32_t get_size() const { return sizeof(sockaddr); }
	string to_string() const;

private:
	
	friend class UDPSocket;
	friend class TCPSocket;

	sockaddr m_sock_addr;

	uint32_t& get_ip4_ref() { return *reinterpret_cast<uint32_t*>(&get_as_sock_addr_in()->sin_addr.S_un.S_addr); }
	const uint32_t& get_ip4_ref() const { return *reinterpret_cast<const uint32_t*>(&get_as_sock_addr_in()->sin_addr.S_un.S_addr); }

	sockaddr_in* get_as_sock_addr_in() { return reinterpret_cast<sockaddr_in*>(&m_sock_addr); }
	const sockaddr_in* get_as_sock_addr_in() const {return reinterpret_cast<const sockaddr_in*>(&m_sock_addr); }
};

typedef shared_ptr <SocketAddress> SocketAddressPtr;

namespace std
{
	template<> struct hash<SocketAddress>
	{
		size_t operator()(const SocketAddress& in_address) const
		{
			return in_address.get_hash();
		}
	};
}
