
--����CS
local CS = {}
_G.CS = CS

CS.ViewMgr = _G.ViewMgr.Inst
_G.ViewMgr = nil
CS.UIUtil = _G.UIUtil
_G.UIUtil = nil
local LuaCallCS = _G.LuaCallCS
CS.LuaCallCS = {
	["Call"] = function (p1, p2)
		LuaCallCS.Call(p1 or "", p2 or "")
	end
}
_G.LuaCallCS = nil

require "Core/TableEx"
require "Core/CoreInclude"
require "Game/GameInclude"

local Test = require "Core/Test"

function Main()
	_G.UI.ViewMgr:Show(_G.UI.ViewID.Login)
end

--场景切换通知
function OnLevelWasLoaded(level)
	collectgarbage("collect")
	Time.timeSinceLevelLoad = 0
end

function OnApplicationQuit()
end