local M = {}

local Config = {
    ["LoginVM"] = {"Game/VM/LoginVM"}
}

local function CreateInner(self, k, v)
    local m = require(v)
    self[k] = m:New()
end

function M:Init()
    for k, v in pairs(Config) do
        CreateInner(self, k, v[1])
    end
end

M:Init()

return M