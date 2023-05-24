
local function Ctor(Class, Obj, ...)
    if Class._Super ~= nil then
        Ctor(Class._Super, Obj, ...)
    end
    if rawget(Class, "Ctor") then
        Class.Ctor(Obj, ...)
    end
end

local function New(Class, ...)
    local Obj = {}
    setmetatable(Obj, {__index = Class})
    Ctor(Class, Obj, ...)
    Obj._Class = Class
    return Obj
end

local function MakeClass(Parent, SupVirtual)
    local Class = {}

    if Parent == nil or type(Parent) ~= "table" then
        Class.New = New
        return Class
    end

    if SupVirtual then
        local FindVal = function(InClass, Key)
            local Raw = rawget(InClass, Key)
            if nil ~= Raw then
                return Raw, InClass
            end
            if nil ~= InClass.__Base then
                return FindVal(InClass.__Base, Key)
            end
        end

        Class.__Base = Parent
        Class.__ClassPtr = Class

        local Index = function(_, Key)
            local Val, ClassPtr = FindVal(Parent, Key)
            if nil == Val then
                return
            end

            Class.__ClassPtr = ClassPtr
            return Val
        end

        setmetatable(Class, {__index = Index})

        local SuperIndex = function(_, Key)
            return function(_, ...)
                local OriClassPtr = Class.__ClassPtr
                if nil == OriClassPtr.__Base then
                    return
                end
                local Val, ClassPtr = FindVal(OriClassPtr.__Base, Key)
                if nil == Val then
                    return
                end
                Class.__ClassPtr = ClassPtr
                local Ret = {Val(Class, ...)}
                Class.__ClassPtr = OriClassPtr
                return table.unpack(Ret)
            end
        end
        Class._Super = setmetatable({}, {__index = SuperIndex})

    else
        setmetatable(Class, {__index = Parent})
        Class._Super = Parent
    end

    Class.New = New
    return Class
end

local function IsA(obj, class)
    if not obj._Class then
        return false
    end

    return obj._Class == class
end

_G.Class = MakeClass
_G.New = New
_G.IsA = IsA

return MakeClass, New

