local M = {}

local dict = {}

function M.Get(path, ...)
    local class = require(path)
    local pool = dict[class]
    if pool ~= nil and #pool > 0 then
        return table.remove(pool)
    end
    return class:New(...)
end

function M.Free(view)
    local class = view._Class
    view:End()
    view:Shutdown()
    local pool = dict[class]
    if pool == nil then
        pool = {}
        dict[class] = pool
    end
    table.insert(pool, view)
end

return M