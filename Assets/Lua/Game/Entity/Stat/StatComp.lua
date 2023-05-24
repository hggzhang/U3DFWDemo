local Comp = _G.Entity.Comp
local StatComp = _G.Class(Comp)
local NumericMgr = require "Game/Numeric/NumericMgr"

function StatComp:Ctor()
    self.numericMgr = NumericMgr:New()
    self.attrs = {}
end

function StatComp:Add(ty, attr)
    if self.attrs[ty] == nil then
        self.attrs[ty] = attr
        self.numericMgr:Add(ty, attr)
    end
end

function StatComp:Remove(ty)
    if self.attrs[ty] ~= nil then
        self.attrs[ty] = nil
        self.numericMgr:Remove(ty)
    end
end

function StatComp:AddMod(ty, mod)
    self.numericMgr:AddMod(ty, mod)
end

function StatComp:RemoveMod(ty, mod)
    self.numericMgr:RemoveMod(ty, mod)
end

function StatComp:Bind(ty, binder)
    self.numericMgr:Bind(ty, binder)
end

function StatComp:UnBind(ty, binder)
    self.numericMgr:UnBind(ty, binder)
end

function StatComp:SetVal(ty, v)
    local n = self.numerics[ty]
    if n == nil then return end
    n:SetVal(v)
end

function StatComp:SetVal(ty, v)
    self.numericMgr:SetVal(ty, v)
end

function StatComp:SetInt(ty, i)
    self.numericMgr:SetInt(ty, i)
end

function StatComp:SetBool(ty, b)
    self.numericMgr:SetBool(ty, b)
end

function StatComp:SetFloat(ty, f)
    self.numericMgr:SetFloat(ty, f)
end

function StatComp:GetVal(ty)
    return self.numericMgr:GetVal(ty)
end

function StatComp:GetInt(ty)
    return self.numericMgr:GetInt(ty)
end

function StatComp:GetBool(ty)
    return self.numericMgr:GetBool(ty)
end

function StatComp:GetFloat(ty)
    return self.numericMgr:GetFloat(ty)
end


return StatComp