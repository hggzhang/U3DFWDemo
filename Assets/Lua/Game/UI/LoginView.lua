local M = _G.Class(_G.UI.View)
local LoginVM = _G.Game.VMConfig.LoginVM
local UIUtil = _G.UI.UIUtil

local UIBinderSetText = _G.UI.Binder.UIBinderSetText
local UIBinderSetImage = _G.UI.Binder.UIBinderSetImage
local LuaCallCS = _G.CS.LuaCallCS
local NetMgr = _G.NetMgr


function M:Ctor()
end

function M:OnInit()
    
end

function M:OnShow()
    self:SetVisible("Check", true)
    self:SetVisible("Pre", false)
    self:SetVisible("Post", false)

    LuaCallCS.Call("CheckHotUpdate")
    
    self.loginNtfHdl = NetMgr:Reg("MsgLoginNtf", function (msg)
        local st = table.tostring_block(msg, 10)
        print(st)

        local code = msg.code

        if code == 0 then
            LoginVM:SetLoginTips("Login failed, press admin both")
            return
        end

        local playerName = msg.playerName
        local gold = msg.gold

        LoginVM:SetGold(gold)
        LoginVM:SetPlayerName(playerName)

        self:SetVisible("Pre", false)
        self:SetVisible("Post", true)
    end)
end

function M:OnHide()
    if self.loginNtfHdl then
        self.loginNtfHdl()
    end
end

function M:OnRegBinders()
    local Binder = {
        {"checkTips",       UIBinderSetText:New(self, "Check/TextCheckTips")},
        {"icon",            UIBinderSetImage:New(self, "Post/ImgHead")},

        {"loginTips",       UIBinderSetText:New(self, "Pre/TextUserTips")},
        {"playerName",       UIBinderSetText:New(self, "Post/TextName")},
        {"gold",       UIBinderSetText:New(self, "Post/TextGold")},
    }

    self:RegBinders(LoginVM, Binder)
end

function M:OnRegEvent()
    self:RegEvent("HotUpdate Check", function (cnt)
        if cnt > 0 then
            local fmt = string.format("检查到{%d}个资源", cnt)
            LoginVM:SetCheckTips(fmt)
        else
            self:SwitchToLogin()
        end
    end)

    self:RegEvent("HotUpdate Update", function (cur, all)
        local fmt = string.format("已下载{%d}kb / 预计{%d}kb, 进度{%f}", cur, all, (cur/all) * 100)
        LoginVM:SetCheckTips(fmt)
    end)

    self:RegEvent("HotUpdate Completed", function ()
        self:SwitchToLogin()
    end)

    self:AddClickEvent("Check/BtnCheck", function ()
        LuaCallCS.Call("StartHotUpdate")
    end)

    self:AddClickEvent("Pre/BtnLogin", function ()
        local user = self:GetText("Pre/InUser");
        local pw = self:GetText("Pre/InPW");

        print("user = " .. tostring(user))
        print("passworld = " .. tostring(pw))

        -- local msgBody = {
        --     ["user"] = "admin",
        --     ["password"] = "admin",
        -- }

        local msgBody = {
            ["user"] = tostring(user),
            ["password"] = tostring(pw),
        }
    
        NetMgr:SendMsg("MsgLogin", msgBody)
    end)
end

function M:SwitchToLogin()
    self:SetVisible("Check", false)
    self:SetVisible("Pre", true)
end

return M