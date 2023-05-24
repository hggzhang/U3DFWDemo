local NumericUtil = {}

local FloatFactor = 100
function NumericUtil.Val2Float(v)
    return v / FloatFactor
end

function NumericUtil.Float2Val(f)
    return math.floor(f * FloatFactor + 0.5)
end

function NumericUtil.Val2Bool(v)
    return v == 1
end

function NumericUtil.Bool2Val(b)
    return b and 1 or 0
end

return NumericUtil