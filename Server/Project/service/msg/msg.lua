
resp = {

}

function resp_msg(name, msgBody)
    if resp[name] and type(resp[name]) == "function" then
        return resp[name](msgBody)
    end
end

function resp_msg_service(service_id, msg)
    local pack = unpack_msg_gate(msg)
    local name, msgBody = process_msg(pack.msg)
    local resp_name, resp_body = resp_msg(name, msgBody)
    if resp_name and resp_body then
        local resp_pack = {
            client_fd = pack.client_fd,
            msg = pack_msg(resp_name, resp_body)
        }
        engine.Send(service_id, pack.gate, m_json.encode(resp_pack))
    end
end

local buff = ""
local len = 0

function process_buff(data)
    buff = buff .. data
    if string.len(buff) < 2 then
        return
    end

    if len == 0 then
        local b1 = string.sub(buff, 1, 1)
        local b2 = string.sub(buff, 2, 2)
        local n1 = string.byte(b1)
        local n2 = string.byte(b2)
        len = n2 * 256 + n1
    end

    if string.len(buff) < len then
        return
    end

    print("process_buff msg len = " .. len)

    local msg = string.sub(buff, 3, -1)
    buff = ""
    len = 0

    return msg
end

function process_msg(data)
    local b1 = string.sub(data, 1, 1)
    local b2 = string.sub(data, 2, 2)
    local n1 = string.byte(b1)
    local n2 = string.byte(b2)
    local l = n2 * 256 + n1
    local name = string.sub(data, 3, 2 + l)
    local body = string.sub(data, 3 + l, -1)

    print("process_msg msg name_len = " .. l)
    print("process_msg msg name = " .. name)
    print("process_msg msg body = " .. body)


    local msgBody = m_json.decode(body)
    return name, msgBody
end

local function pack_len(len)
    local n1 = len % 256
    local n2 = len // 256
    local b1 = string.char(n1)
    local b2 = string.char(n2)

    return b1 .. b2
end

function pack_msg(name, msg)
    local ln = string.len(name)
    local lns = pack_len(ln)

    local msgBody = m_json.encode(msg)
    local l = ln + string.len(msgBody) + 4
    local ls = pack_len(l)
    return ls .. lns .. name .. msgBody
end


function unpack_msg_service(serviceMsg)
    local pack = m_json.decode(serviceMsg)
    return pack.client_fd, pack.msg
end

function unpack_msg_gate(serviceMsg)
    return  m_json.decode(serviceMsg)
end


