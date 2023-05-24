#pragma once

class SocketAddressFactory
{
public:
	static SocketAddressPtr create_IPv4_from_string(const string& inString);
};
