--------------------------------------------------------------------------------
--      Copyright (c) 2015 - 2016 , 蒙占志(topameng) topameng@gmail.com
--      All rights reserved.
--      Use, modification and distribution are subject to the "MIT License"
--------------------------------------------------------------------------------
-- if jit then		
-- 	if jit.opt then		
-- 		jit.opt.start(3)				
-- 	end		
	
-- 	print("ver"..jit.version_num.." jit: ", jit.status())
-- 	print(string.format("os: %s, arch: %s", jit.os, jit.arch))
-- end

-- if DebugServerIp then  
--   require("mobdebug").start(DebugServerIp)
-- end

require 				"LuaFramework/ToLua/Lua/misc.functions"
Mathf		= require "LuaFramework/ToLua/Lua/UnityEngine.Mathf"
Vector3 	= require "LuaFramework/ToLua/Lua/UnityEngine.Vector3"
Quaternion	= require "LuaFramework/ToLua/Lua/UnityEngine.Quaternion"
Vector2		= require "LuaFramework/ToLua/Lua/UnityEngine.Vector2"
Vector4		= require "LuaFramework/ToLua/Lua/UnityEngine.Vector4"
Color		= require "LuaFramework/ToLua/Lua/UnityEngine.Color"
Ray			= require "LuaFramework/ToLua/Lua/UnityEngine.Ray"
Bounds		= require "LuaFramework/ToLua/Lua/UnityEngine.Bounds"
RaycastHit	= require "LuaFramework/ToLua/Lua/UnityEngine.RaycastHit"
Touch		= require "LuaFramework/ToLua/Lua/UnityEngine.Touch"
LayerMask	= require "LuaFramework/ToLua/Lua/UnityEngine.LayerMask"
Plane		= require "LuaFramework/ToLua/Lua/UnityEngine.Plane"
Time		= reimport "LuaFramework/ToLua/Lua/UnityEngine.Time"

list		= require "LuaFramework/ToLua/Lua/list"
utf8		= require "LuaFramework/ToLua/Lua/misc.utf8"

require "LuaFramework/ToLua/Lua/event"
require "LuaFramework/ToLua/Lua/typeof"
require "LuaFramework/ToLua/Lua/slot"
require "LuaFramework/ToLua/Lua/System.Timer"
require "LuaFramework/ToLua/Lua/System.coroutine"
require "LuaFramework/ToLua/Lua/System.ValueType"
require "LuaFramework/ToLua/Lua/System.Reflection.BindingFlags"

-- require "misc.strict"