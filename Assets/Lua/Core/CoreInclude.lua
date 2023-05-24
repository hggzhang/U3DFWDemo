
_G.Log = require "Core/Log"

local Core = {}
_G.Core = Core

require "Core/LuaClass"
_G.MgrBase = require "Core/MgrBase"
_G.EventMgr = require "Core/EventMgr"
_G.NetMgr = require "Core/NetMgr"

Core.ObjPool = require "Core/ObjPool"
Core.Binder = require "Core/Binder"
Core.BinderRegister = require "Core/BinderRegister"
Core.BindableProperty = require "Core/BindableProperty"
Core.BindableDec = require "Core/BindableDec"

Core.EventRegister = require "Core/EventRegister"
Core.TimerRegister = require "Core/TimerRegister"


local UI = {}
_G.UI = UI
UI.UIUtil = require "Core/UI/UIUtil"
UI.ViewID = require "Core/UI/ViewID"
UI.ViewConfig = require "Core/UI/ViewConfig"
UI.VM = require "Core/UI/VM"
UI.ViewFactory = require "Core/UI/ViewFactory"
UI.View = require "Core/UI/View"
UI.ViewMgr = require "Core/UI/ViewMgr"
UI.BindableList = require "Core/UI/BindableList"

local UIAdapter = {}
UI.Adapter = UIAdapter
UIAdapter.ListViewAdapter = require "Core/UI/Adapter/ListViewAdapter"

local UIBinder = {}
UI.Binder = UIBinder
UIBinder.UIBinder = require "Core/UI/Binder/UIBinder"
UIBinder.UIBinderSetImage = require "Core/UI/Binder/UIBinderSetImage"
UIBinder.UIBinderSetText = require "Core/UI/Binder/UIBinderSetText"


_G.CSCallEve = function (k, ...)
    _G.EventMgr:Send(k, ...)
end

_G.CSNetMsg = function (code)
    _G.NetMgr:OnRecv(code)
end





