local NumericMgr = _G.Class()

function NumericMgr:Ctor()
    self.numerics = {}
end

function NumericMgr:Init()
    -- body
end

function NumericMgr:Add(ty, n)
    if self.numerics[ty] ~= nil then return end
    self.numerics[ty] = n
end

function NumericMgr:Remove(ty)
    if self.numerics[ty] == nil then return end
    self.numerics[ty] = nil
end

function NumericMgr:Get(ty)
    return self.numerics[ty]
end

function NumericMgr:AddMod(ty, mod)
    local n = self.numerics[ty]
    if n == nil then return end
    n:AddMod(mod)
end

function NumericMgr:RemoveMod(ty, mod)
    local n = self.numerics[ty]
    if n == nil then return end
    n:RemoveMod(mod)
end

function NumericMgr:Bind(ty, binder)
    local n = self.numerics[ty]
    if n == nil then return end
    n:Bind(binder)
end

function NumericMgr:UnBind(ty, binder)
    local n = self.numerics[ty]
    if n == nil then return end
    n:UnBind(binder)
end

function NumericMgr:SetVal(ty, v)
    local n = self.numerics[ty]
    if n == nil then return end
    n:SetVal(v)
end

function NumericMgr:SetVal(ty, v)
    local n = self.numerics[ty]
    if n == nil then return end
    n:SetVal(v)
end

function NumericMgr:SetInt(ty, i)
    local n = self.numerics[ty]
    if n == nil then return end
    n:SetInt(i)
end

function NumericMgr:SetBool(ty, b)
    local n = self.numerics[ty]
    if n == nil then return end
    n:SetBool(b)
end

function NumericMgr:SetFloat(ty, f)
    local n = self.numerics[ty]
    if n == nil then return end
    n:SetFloat(f)
end

function NumericMgr:GetVal(ty)
    local n = self.numerics[ty]
    if n == nil then return end
    return n:GetVal()
end

function NumericMgr:GetInt(ty)
    local n = self.numerics[ty]
    if n == nil then return end
    return n:GetInt()
end

function NumericMgr:GetBool(ty)
    local n = self.numerics[ty]
    if n == nil then return end
    return n:GetBool()
end

function NumericMgr:GetFloat(ty)
    local n = self.numerics[ty]
    if n == nil then return end
    return n:GetFloat()
end

return NumericMgr