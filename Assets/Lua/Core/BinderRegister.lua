local BinderRegister = _G.Class()

function BinderRegister:Ctor()
    self.data = {}
end

function BinderRegister:RegBinders(bindableObj, binders)
    if self.data[bindableObj] == nil then
        self.data[bindableObj] = {}
    end

    for _, binderInfo in pairs(binders) do
        local key = binderInfo[1]
        local binder = binderInfo[2]
        if self.data[bindableObj][key] == nil then
            self.data[bindableObj][key] = binder
            bindableObj:Bind(key, binder)
        end
    end
end

function BinderRegister:UnRegBinders(bindableObj, binders)
    if self.data[bindableObj] == nil then
        return
    end

    for _, binderInfo in pairs(binders) do
        local key = binderInfo[1]
        local binder = binderInfo[2]
        if self.data[bindableObj][key] ~= nil then
            self.data[bindableObj][key] = nil
            bindableObj:UnBind(key, binder)
        end
    end
end

function BinderRegister:RegBinder(bindableObj, key, binder)
    if self.data[bindableObj] == nil then
        self.data[bindableObj] = {}
    end

    if self.data[bindableObj][key] == nil then
        self.data[bindableObj][key] = binder
        bindableObj:Bind(key, binder)
    end
end

function BinderRegister:UnRegBinder(bindableObj, key, binder)
    if self.data[bindableObj] == nil then
        return
    end

    if self.data[bindableObj][key] == nil then
        return
    end

    self.data[bindableObj][key] = binder
    bindableObj:UnBind(key, binder)
end

function BinderRegister:UnRegAllBinders()
    for obj, objBinders in pairs(self.data) do
        for key, binder in pairs(objBinders) do
            obj:UnBind(key, binder)
        end
    end
    table.clear(self.data)
end

return BinderRegister