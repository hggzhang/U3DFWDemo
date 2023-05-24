local BindableProperty = _G.Core.BindableProperty

local function BindableDec(class)
    class.Bind = function(obj, k, binder)
        local bindableProperty = obj._bindablePropertys[k]
        if bindableProperty then
            bindableProperty:Bind(binder)
        end
    end

    class.UnBind = function(obj, k, binder)
        local bindableProperty = obj._bindablePropertys[k]
        if bindableProperty then
            bindableProperty:UnBind(binder)
        end
    end

    class._bindablePropertys = {}
    local meta = getmetatable(class)
    local metaIndex = nil
    if meta then
        metaIndex = meta.__index
    end
    local newMeta = {
        __index = function (t, k)
            if t._bindablePropertys[k] ~= nil then
                return t._bindablePropertys[k].val
            else
                if metaIndex then
                    return metaIndex[k]
                end
            end
        end,

        __newindex = function(t, k, v)
            if type(v) == "function" then
                rawset(t, k, v)
            else
                if t._bindablePropertys[k] == nil then
                    t._bindablePropertys[k] = BindableProperty:New(v)
                end
                t._bindablePropertys[k]:Modify(v)
            end
        end
    }
    setmetatable(class, newMeta)
end

return BindableDec