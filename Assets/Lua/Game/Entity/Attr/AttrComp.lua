local Comp = _G.Entity.Comp
local AttrComp = _G.Class(Comp)
local NumericMgr = require "Game/Numeric/NumericMgr"

function AttrComp:Ctor()
    self.numericMgr = NumericMgr:New()
    self.attrs = {}
end

function AttrComp:Add(ty, attr)
    if self.attrs[ty] == nil then
        self.attrs[ty] = attr
        self.numericMgr:Add(ty, attr)
    end
end

function AttrComp:Remove(ty)
    if self.attrs[ty] ~= nil then
        self.attrs[ty] = nil
        self.numericMgr:Remove(ty)
    end
end

function AttrComp:AddMod(ty, mod)
    self.numericMgr:AddMod(ty, mod)
end

function AttrComp:RemoveMod(ty, mod)
    self.numericMgr:RemoveMod(ty, mod)
end

function AttrComp:Bind(ty, binder)
    self.numericMgr:Bind(ty, binder)
end

function AttrComp:UnBind(ty, binder)
    self.numericMgr:UnBind(ty, binder)
end

function AttrComp:SetVal(ty, v)
    local n = self.numerics[ty]
    if n == nil then return end
    n:SetVal(v)
end

function AttrComp:SetVal(ty, v)
    self.numericMgr:SetVal(ty, v)
end

function AttrComp:SetInt(ty, i)
    self.numericMgr:SetInt(ty, i)
end

function AttrComp:SetBool(ty, b)
    self.numericMgr:SetBool(ty, b)
end

function AttrComp:SetFloat(ty, f)
    self.numericMgr:SetFloat(ty, f)
end

function AttrComp:GetVal(ty)
    return self.numericMgr:GetVal(ty)
end

function AttrComp:GetInt(ty)
    return self.numericMgr:GetInt(ty)
end

function AttrComp:GetBool(ty)
    return self.numericMgr:GetBool(ty)
end

function AttrComp:GetFloat(ty)
    return self.numericMgr:GetFloat(ty)
end


return AttrComp