
local Numeric = _G.Class()
local NumericUtil = _G.Game.Numeric.NumericUtil
local NumericModInc = require "Game/Numeric/NumericModInc"

function Numeric:Ctor(val)
    self.val = val or 0
    self.total = self.val
    self.mods = {}
    self.binders = {}
    self.mod = NumericModInc:New()
    self.mod:SetVal(self.total)
end

function Numeric:SetVal(val)
    local old = self.total
    self.val = val
    self.total = val
    self:Modify()
    if old == self.total then return end
    self:OnValChanged(self.total, old)
end

function Numeric:Modify()
    for i = 1, #self.mods do
        self.mods[i]:Modify()
    end
end

function Numeric:OnValChanged(new, old)
    self.mod:SetVal(new)
    for _, binder in ipairs(self.binders) do
        binder:OnValChanged(new, old)
    end
end

function Numeric:Update()
    local old = self.total
    self:Modify()
    if old == self.total then return end
    self:OnValChanged(self.total, old)
end

function Numeric:AddMod(mod)
    table.insert(self.mods, mod)
    mod:SetNumeric(self)
    table.sort(self.mods, function(a, b)
        return a.ty < b.ty
    end)
    local old = self.total
    self:Modify()
    if old == self.total then return end
    self:OnValChanged(self.total, old)
end

function Numeric:RemoveMod(mod)
    table.remove_item(self.mods, mod)
    mod:SetNumeric(nil)
    local old = self.total
    self:Modify()
    if old == self.total then return end
    self:OnValChanged(self.total, old)
end

function Numeric:Bind(binder)
    table.insert(self.binders, binder)
    binder:OnValChanged(self.total)
end

function Numeric:UnBind(binder)
    table.remove_item(self.binders, binder)
end

function Numeric:SetBool(b)
    self:SetVal(NumericUtil.Bool2Val(b))
end

function Numeric:SetInt(i)
    self:SetVal(i)
end

function Numeric:SetFloat(f)
    self:SetVal(NumericUtil.Float2Val(f))
end

function Numeric:GetVal()
    return self.total
end

function Numeric:GetBool()
    return NumericUtil.Val2Bool(self:GetVal())
end

function Numeric:GetInt()
    return self:GetVal()
end

function Numeric:GetFloat()
    return NumericUtil.Val2Float(self:GetVal())
end

return Numeric

