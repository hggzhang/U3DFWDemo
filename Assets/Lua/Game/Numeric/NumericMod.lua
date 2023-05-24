local NumericDefine = _G.Game.Numeric.NumericDefine

local NumericMod = _G.Class()

function NumericMod:Ctor(numeric, val)
    self.numeric = numeric
    self.val = val or 0
    self.ty = NumericDefine.ENumericType.None
end

function NumericMod:SetNumeric(numeric)
    self.numeric = numeric
end

function NumericMod:SetVal(val)
    if self.val == val then return end
    self.val = val or 0
    if self.numeric ~= nil then
        self.numeric:Update()
    end
end

function NumericMod:Modify()
    --
end

return NumericMod