local NumericMod = require "Game/Numeric/NumericMod"

local NumericModInc = _G.Class(NumericMod)

function NumericModInc:Modify()
    local total = self.numeric.total
    self.numeric.total = total + self.val
end

return NumericModInc