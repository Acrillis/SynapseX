--[[
    SYNAPSE X
    File.:	InitScript.lua
	Desc.:	Lazy init script for shit that im too lazy to do in C++
]]

local HtType = true

local InsertService = game:GetService'InsertService'
local LoadAsset = InsertService.LoadLocalAsset
local getrawmetatable = getrawmetatable
local TableRemove = table.remove
local TableInsert = table.insert
local unpack = unpack
local GameMeta = getrawmetatable(game)
setreadonly(GameMeta, false)
local GameIndex = GameMeta.__index
local GameNamecall = GameMeta.__namecall
local GameToString = GameMeta.__tostring
local HtGet
local HtPost
local HtForceGet = game.HttpGetAsync
if HtType then
    HtGet = game.HttpGetAsync
    HtPost = game.HttpPostAsync
else
    HtGet = httpget
    HtPost = httppost
end
local GetNDM = getndm
local SetNDM = setndm
local GetInstanceList = getinstancelist
local CheckC = checkcaller
local NewCC = newcclosure
local ReATT = reattach
local HookFunc = hookfunction
local GetLoadedModules = getloadedmodules
local StrFind = string.find
local StrGSub = string.gsub
local RepKick = reportkick
local Req = getrenv().require

getgenv().newcclosure = NewCC(function(f)
    if type(f) ~= "function" then error("expected function as argument #1") end
    if not islclosure(f) then error("expected lua function as argument #1") end

    local n = 1
    for i,v in pairs(debug.getupvalues(f)) do
        debug.setupvaluename(f, n, "?")
        n = n + 1
    end

    return NewCC(f)
end)

local GetObjects = newcclosure(function(self, a)
    local Objects = {}
    if a then
        local b = LoadAsset(InsertService, a)
        if b then TableInsert(Objects, b) end
    end
    return Objects
end)

--stupid hack for HttpGet/Post
local HttpGet = newcclosure(function(self, url)
    local GND = GetNDM(game)
    SetNDM(game, 3)
    spawn(function() SetNDM(game, GND) end)
    if HtType then
        return HtGet(game, url)
    else
        return HtGet(url)
    end
end)

local HttpPost = newcclosure(function(self, url, content, _, req)
    local GND = GetNDM(game)
    SetNDM(game, 3)
    spawn(function() SetNDM(game, GND) end)
    if req == nil then req = "*/*" end
    if HtType then
        return HtPost(game, url, content, req)
    else
        if req == nil then req = "text/plain" end
        return HtPost(url, content, req)
    end
end)

local HttpGetAs = newcclosure(function(g, url, ...)
    local GND = GetNDM(game)
    SetNDM(game, 3)
    spawn(function() SetNDM(game, GND) end)
    if HtType then
        return HtGet(g, url, ...)
    else
        local Args = {g, url, ...}
        return HtGet(Args[2])
    end
end)

local HttpPostAs = newcclosure(function(g, url, ...)
    local GND = GetNDM(game)
    SetNDM(game, 3)
    spawn(function() SetNDM(game, GND) end)
    if HtType then
        return HtPost(g, url, ...)
    else
        local Args = {g, url, ...}
        if Args[4] == nil then Args[4] = "text/plain" end
        return HtPost(Args[2], Args[3], Args[4])
    end
end)


local MarketService = game:GetService("MarketplaceService")
local FindString = string.find
local ReverseString = string.reverse

local MarketInitBlacklist =
{
    ["PerformPurchase"] = true,
    ["PromptBundlePurchase"] = true,
    ["PromptGamePassPurchase"] = true,
    ["PromptNativePurchase"] = true,
    ["PromptProductPurchase"] = true,
    ["PromptPurchase"] = true,
    ["PromptThirdPartyPurchase"] = true
}

local MarketBlacklist = {}

for I, _ in pairs(MarketInitBlacklist) do
	MarketBlacklist[ReverseString(I)] = true
end

MarketInitBlacklist = {}

setreadonly(MarketBlacklist, true)

local MarketError = newcclosure(function()
    error("MarketplaceService functions are not supported for security reasons in Synapse.")
    while true do end
end)

local LuaHookMap = {}
local GetInfo = debug.getinfo

local GService = game.GetService
local PCall = pcall

local MetaHandle = newcclosure(function(self, a)
    if self == game then
        if a == 'GetObjects' then return GetObjects end
        if a == "HttpGet" then return HttpGet end
        if a == "HttpPost" then return HttpPost end
        
		--if a == "HttpGetAsync" then return HttpGetAs end  fat crash
        --if a == "HttpPostAsync" then return HttpPostAs end  fat crash
        if a == "HttpGetAsync" then return HttpGet end
        if a == "HttpPostAsync" then return HttpPost end

		if a == "OpenVideosFolder" or a == "OpenScreenshotsFolder" then 
			return newcclosure(function()
				error("Dangerous function '" .. a .. "' is not supported for security reasons in Synapse.")
				while true do end
			end)
		end

		local E, R = PCall(GService, self, a)
		if E then return R end
    end

    if self == MarketService then
        for I, _ in pairs(MarketBlacklist) do
            if FindString(a, ReverseString(I)) then return MarketError end
        end
    end
end)

local CheckRL = checkrbxlocked
local CheckIN = checkinst
local CheckPC = checkparentchain
local GetCN = getclassname
local GetNCM = getnamecallmethod

GameMeta.__index = newcclosure(function(self, a)
    if CheckC() then
        local b = MetaHandle(self, a)
        if b ~= nil then return b end
    end
    return GameIndex(self, a)
end)

if isluau() then
    GameMeta.__namecall = newcclosure(function(self, ...)
        if CheckC() then
            local a = GetNCM()
            local b = MetaHandle(self, a)
            if b ~= nil then return b(self, ...) end
        end
        return GameNamecall(self, ...)
    end)
else
    GameMeta.__namecall = newcclosure(function(self, ...)
        if CheckC() then
            local a = {...}
            local b = TableRemove(a)
            local c = MetaHandle(self, b)
            if c ~= nil then return c(self, unpack(a)) end
        end
        return GameNamecall(self, ...)
    end)
end


--[[GameMeta.__tostring = newcclosure(function(self)
    --Synapse X CoreGui Protection - we return the ClassName of certian RobloxLocked objects to prevent stupid detection methods
    --ex game that does this: CB:RO
    if not CheckC() and typeof(self) == "Instance" and CheckRL(self) and not CheckIN(self, CGui) and CheckPC(self, CGui) and not CheckPC(self, RGui) and not CheckPC(self, RLGui) and not CheckPC(self, DCGui) then
        local TS = GameToString(self)
		if TS == "DevConsoleMaster" or TS == "RobloxLoadingGui" then return TS end

		local Ret = GetCN(self)
        if BListInst[Ret] then return "TextLabel" else return Ret end
    end

    return GameToString(self)
end)]]

setreadonly(GameMeta, true)

--too lazy to implement in C++
getgenv().getnilinstances = newcclosure(function()
    local inst = GetInstanceList()
    local r = {}

    for i, v in pairs(inst) do
        if typeof(v) == "Instance" and v.Parent == nil then 
            r[#r + 1] = v 
        end
    end

    return r
end)

getgenv().get_nil_instances = getnilinstances

getgenv().getinstances = newcclosure(function()
    local inst = GetInstanceList()
    local r = {}

    for i, v in pairs(inst) do
        if typeof(v) == "Instance" then 
            r[#r + 1] = v 
        end
    end

    return r
end)

getgenv().get_instances = getinstances

getgenv().getscripts = newcclosure(function()
    local inst = GetInstanceList()
    local r = {}

    for i, v in pairs(inst) do
        if typeof(v) == "Instance" and (v:IsA("LocalScript") or v:IsA("ModuleScript")) and not v.RobloxLocked then 
            r[#r + 1] = v 
        end
    end

    return r
end)

getgenv().get_scripts = getscripts

--hookfunction w/ more safety checks
getgenv().hookfunction = newcclosure(function(old, new)
    if type(old) ~= "function" then error("expected function as argument #1") end
    if type(new) ~= "function" then error("expected function as argument #2") end

    if islclosure(old) and not islclosure(new) then error("expected C function or Lua function as both argument #1 and #2") end

    local hook
    if not islclosure(old) and islclosure(new) then 
		hook = newcclosure(new)
    else
        hook = new
    end

    return HookFunc(old, hook)
end)

getgenv().hookfunc = hookfunction
getgenv().replaceclosure = hookfunction

--unlockmodulescript is useless, isn't needed in Synapse X
getgenv().unlockmodulescript = newcclosure(function() end)

--require fix
getgenv().require = function(scr)
	if typeof(scr) ~= 'Instance' or scr.ClassName ~= 'ModuleScript' then error'attempt to require a non-ModuleScript' end
	if CheckRL(scr) then error'attempt to require a core ModuleScript' end
    local oIdentity = syn.get_thread_identity()

    syn.set_thread_identity(2)
    local g, res = pcall(Req, scr)
    syn.set_thread_identity(oIdentity)

    if not g then 
        error(res) 
    end

    return res
end

--getloadedmodules fix
getgenv().getloadedmodules = newcclosure(function()
    local Unfiltered = GetLoadedModules()
    local Filtered = {}

    for I,V in pairs(Unfiltered) do
        if not CheckRL(V) then table.insert(Filtered, V) end
    end

    return Filtered
end)

getgenv().get_loaded_modules = getloadedmodules

--internal: forced HttpGetAsync
getgenv().htgetf = newcclosure(function(url)
    local GND = GetNDM(game)
    SetNDM(game, 3)
    spawn(function() SetNDM(game, GND) end)
    return HtForceGet(game, url)
end)

--output redirection
local RedirectEnabled = is_redirection_enabled
local PrintConsole = printconsole

--[[
local FireMsgOut = firemessageout
local CreateMsgOutString = createmsgoutstring
local HookErrHandlers = hookerrorhandlers
local MsgOutRedirect = Instance.new("BindableEvent")
local UseMsgOutRedirect = false
local MsgOut = game:GetService("LogService").MessageOut]]

--[[getgenv().getmessageout = newcclosure(function()
    UseMsgOutRedirect = true

    return MsgOutRedirect.Event
end)]]

local Print = getrenv().print
local Warn = getrenv().warn

getgenv().print = newcclosure(function(...)
  local Rt, Rn = {...}, select('#', ...)
	local Str = ""
	for i = 1, Rn do
        local v = tostring(Rt[i])
        if (type(v) ~= "string") then
            error("'tostring' must return a string to 'print'", 0)
        end

		Str = Str .. v .. " "
    end

    --[[if UseMsgOutRedirect then
        MsgOutRedirect:Fire(Str, Enum.MessageType.MessageOutput)
    end]]
    
	if RedirectEnabled() then
		return printconsole(Str, 255, 255, 255)
    else
        return Print(Str)
    end
end)

getgenv().warn = newcclosure(function(...)
  local Rt, Rn = {...}, select('#', ...)
	local Str = ""
	for i = 1, Rn do
        local v = tostring(Rt[i])
        if (type(v) ~= "string") then
            error("'tostring' must return a string to 'warn'", 0)
        end

		Str = Str .. v .. " "
    end

	 --[[
    if UseMsgOutRedirect then
        MsgOutRedirect:Fire(Str, Enum.MessageType.MessageWarning)
    end]]
    
	if RedirectEnabled() then
		return printconsole(Str, 255, 218, 68)
    else
        return Warn(Str)
    end
end)

--[[spawn(function()
    local SynErr = Instance.new("BindableEvent")
    local LastMsg = ""

    SynErr.Event:Connect(function(Err)
        if LastMsg == Err then return end
        LastMsg = Err

        if UseMsgOutRedirect then
            MsgOutRedirect:Fire(Err, Enum.MessageType.MessageError)
        end

        if is_redirection_enabled() then
            printconsole(Err, 215, 90, 74)
        else
            return FireMsgOut(MsgOut, "MessageError", Err)
        end
    end)

    getreg()[CreateMsgOutString()] = SynErr

    game:GetService("RunService").Heartbeat:Connect(HookErrHandlers)
end)]]

--drawing API
local GetRP = getrenderproperty
local SetRP = setrenderproperty
local CreateRP = createrenderobject
local DestroyRP = destroyrenderobject

local LineMT =
{
    __index = function(T, K)
        if not rawget(T, "__OBJECT_EXISTS") then error("render object destroyed") end

        if K == "Remove" then return newcclosure(function() DestroyRP(rawget(T, "__OBJECT")) rawset(T, "__OBJECT", nil) rawset(T, "__OBJECT_EXISTS", false) end) end

        return GetRP(rawget(T, "__OBJECT"), K)
    end,

    __newindex = function(T, K, V)
        if not rawget(T, "__OBJECT_EXISTS") then error("render object destroyed") end

        if K == "Visible" and type(V) ~= "boolean" then error("invalid type '" .. typeof(V) .. "' for property 'Visible', expected boolean") end
        if K == "From" and typeof(V) ~= "Vector2" then error("invalid type '" .. typeof(V) .. "' for property 'From', expected Vector2") end
        if K == "To" and typeof(V) ~= "Vector2" then error("invalid type '" .. typeof(V) .. "' for property 'To', expected Vector2") end
        if K == "Color" and typeof(V) ~= "Color3" then error("invalid type '" .. typeof(V) .. "' for property 'Color', expected Color3") end
        if K == "Thickness" and type(V) ~= "number" then error("invalid type '" .. typeof(V) .. "' for property 'Thickness', expected number") end
        if K == "Transparency" and type(V) ~= "number" then error("invalid type '" .. typeof(V) .. "' for property 'Transparency', expected number") end

        return SetRP(rawget(T, "__OBJECT"), K, V)
    end,

    __type = "Line"
}

local TextMT = 
{
    __index = function(T, K)
        if not rawget(T, "__OBJECT_EXISTS") then error("render object destroyed") end

        if K == "Remove" then return newcclosure(function() DestroyRP(rawget(T, "__OBJECT")) rawset(T, "__OBJECT", nil) rawset(T, "__OBJECT_EXISTS", false) end) end

        return GetRP(rawget(T, "__OBJECT"), K)
    end,

    __newindex = function(T, K, V)
        if not rawget(T, "__OBJECT_EXISTS") then error("render object destroyed") end

        if K == "Visible" and type(V) ~= "boolean" then error("invalid type '" .. typeof(V) .. "' for property 'Visible', expected boolean") end
        if K == "Text" and type(V) ~= "string" then error("invalid type '" .. typeof(V) .. "' for property 'Text', expected string") end
        if K == "Position" and typeof(V) ~= "Vector2" then error("invalid type '" .. typeof(V) .. "' for property 'Position', expected Vector2") end
        if K == "Color" and typeof(V) ~= "Color3" then error("invalid type '" .. typeof(V) .. "' for property 'Color', expected Color3") end
        if K == "Center" and type(V) ~= "boolean" then error("invalid type '" .. typeof(V) .. "' for property 'Center', expected boolean") end
        if K == "Outline" and type(V) ~= "boolean" then error("invalid type '" .. typeof(V) .. "' for property 'Outline', expected boolean") end
        if K == "OutlineColor" and typeof(V) ~= "Color3" then error("invalid type '" .. typeof(V) .. "' for property 'OutlineColor', expected Color3") end
		if K == "Size" and type(V) ~= "number" then error("invalid type '" .. typeof(V) .. "' for property 'Size', expected number") end
        if K == "Font" and type(V) ~= "number" then error("invalid type '" .. typeof(V) .. "' for property 'Font', expected number") end
        if K == "Font" and (V < 0 or V > 3) then error("invalid font") end
        if K == "Transparency" and type(V) ~= "number" then error("invalid type '" .. typeof(V) .. "' for property 'Transparency', expected number") end

        return SetRP(rawget(T, "__OBJECT"), K, V)
    end,

    __type = "Text"
}

local SquareMT = 
{
    __index = function(T, K)
        if not rawget(T, "__OBJECT_EXISTS") then error("render object destroyed") end

        if K == "Remove" then return newcclosure(function() DestroyRP(rawget(T, "__OBJECT")) rawset(T, "__OBJECT", nil) rawset(T, "__OBJECT_EXISTS", false) end) end

        return GetRP(rawget(T, "__OBJECT"), K)
    end,

    __newindex = function(T, K, V)
        if not rawget(T, "__OBJECT_EXISTS") then error("render object destroyed") end

        if K == "Visible" and type(V) ~= "boolean" then error("invalid type '" .. typeof(V) .. "' for property 'Visible', expected boolean") end
        if K == "Position" and typeof(V) ~= "Vector2" then error("invalid type '" .. typeof(V) .. "' for property 'Position', expected Vector2") end
        if K == "Size" and typeof(V) ~= "Vector2" then error("invalid type '" .. typeof(V) .. "' for property 'Size', expected Vector2") end
        if K == "Color" and typeof(V) ~= "Color3" then error("invalid type '" .. typeof(V) .. "' for property 'Color', expected Color3") end
        if K == "Thickness" and type(V) ~= "number" then error("invalid type '" .. typeof(V) .. "' for property 'Thickness', expected number") end
        if K == "Filled" and type(V) ~= "boolean" then error("invalid type '" .. typeof(V) .. "' for property 'Filled', expected boolean") end
        if K == "Transparency" and type(V) ~= "number" then error("invalid type '" .. typeof(V) .. "' for property 'Transparency', expected number") end

        return SetRP(rawget(T, "__OBJECT"), K, V)
    end,

    __type = "Square"
}

local CircleMT = 
{
    __index = function(T, K)
        if not rawget(T, "__OBJECT_EXISTS") then error("render object destroyed") end

        if K == "Remove" then return newcclosure(function() DestroyRP(rawget(T, "__OBJECT")) rawset(T, "__OBJECT", nil) rawset(T, "__OBJECT_EXISTS", false) end) end

        return GetRP(rawget(T, "__OBJECT"), K)
    end,

    __newindex = function(T, K, V)
        if not rawget(T, "__OBJECT_EXISTS") then error("render object destroyed") end

        if K == "Visible" and type(V) ~= "boolean" then error("invalid type '" .. typeof(V) .. "' for property 'Visible', expected boolean") end
        if K == "Position" and typeof(V) ~= "Vector2" then error("invalid type '" .. typeof(V) .. "' for property 'Position', expected Vector2") end
        if K == "Radius" and type(V) ~= "number" then error("invalid type '" .. typeof(V) .. "' for property 'Radius', expected number") end
        if K == "Color" and typeof(V) ~= "Color3" then error("invalid type '" .. typeof(V) .. "' for property 'Color', expected Color3") end
        if K == "Thickness" and type(V) ~= "number" then error("invalid type '" .. typeof(V) .. "' for property 'Thickness', expected number") end
        if K == "Filled" and type(V) ~= "boolean" then error("invalid type '" .. typeof(V) .. "' for property 'Filled', expected boolean") end
        if K == "Transparency" and type(V) ~= "number" then error("invalid type '" .. typeof(V) .. "' for property 'Transparency', expected number") end
        if K == "NumSides" and type(V) ~= "number" then error("invalid type '" .. typeof(V) .. "' for property 'NumSides', expected number") end

        return SetRP(rawget(T, "__OBJECT"), K, V)
    end,

    __type = "Circle"
}

local RPInit = false

local Draw =
{
    new = function(Type)
        if Type == "Line" then
            local RendObj = CreateRP(Type)
            if not RPInit then wait(0.1) RPInit = true end

            local Ret = 
            {
                __OBJECT = RendObj,
                __OBJECT_EXISTS = true
            }

            return setmetatable(Ret, LineMT)
        end

        if Type == "Text" then
            local RendObj = CreateRP(Type)
            if not RPInit then wait(0.1) RPInit = true end

            local Ret = 
            {
                __OBJECT = RendObj,
                __OBJECT_EXISTS = true
            }

            return setmetatable(Ret, TextMT)
        end

        if Type == "Square" then
            local RendObj = CreateRP(Type)
            if not RPInit then wait(0.1) RPInit = true end

            local Ret = 
            {
                __OBJECT = RendObj,
                __OBJECT_EXISTS = true
            }

            return setmetatable(Ret, SquareMT)
        end

        if Type == "Circle" then
            local RendObj = CreateRP(Type)
            if not RPInit then wait(0.1) RPInit = true end

            local Ret = 
            {
                __OBJECT = RendObj,
                __OBJECT_EXISTS = true
            }

            return setmetatable(Ret, CircleMT)
        end

        error("invalid object type ('" .. tostring(Type) .. "')")
    end,

    Fonts =
    {
        UI = 0,
        System = 1,
        Plex = 2,
        Monospace = 3
    }
}

--event API
local GetCons = getconnections
local DisableCon = disableconnection
local EnableCon = enableconnection
local GetConFunc = getconnectionfunc
local GetConnectState = getconnectionstate

local ConnMT =
{
    __index = function(T, K)
        if K == "Disable" then 
            return newcclosure(function()
                if not rawget(T, "__OBJECT_ENABLED") then return end
                DisableCon(rawget(T, "__OBJECT"))
                rawset(T, "__OBJECT_ENABLED", false)
            end) 
        end

        if K == "Enable" then 
            return newcclosure(function()
                if rawget(T, "__OBJECT_ENABLED") then return end
                EnableCon(rawget(T, "__OBJECT"))
                rawset(T, "__OBJECT_ENABLED", true)
            end) 
        end

        if K == "Enabled" then
            return rawget(T, "__OBJECT_ENABLED")
        end

		if K == "State" then
            return GetConnectState(rawget(T, "__OBJECT"))
        end

		if K == "Fire" then
			return newcclosure(function(self, ...)
				return GetConFunc(rawget(T, "__OBJECT"))(...)
			end)
		end

		if K == "Function" then
			return GetConFunc(rawget(T, "__OBJECT"))
		end

        return nil
    end,

    __newindex = function(T, K, V)
        if K == "Enabled" then
            if V then
                if rawget(T, "__OBJECT_ENABLED") then return end
                EnableCon(rawget(T, "__OBJECT"))
                rawset(T, "__OBJECT_ENABLED", true)
            else
                if not rawget(T, "__OBJECT_ENABLED") then return end
                DisableCon(rawget(T, "__OBJECT"))
                rawset(T, "__OBJECT_ENABLED", false)
            end
        end
    end,

    __type = "Event"
}

local ConCache = {}
getgenv().getconnections = newcclosure(function(sig)
    local Cons = GetCons(sig)
    local Ret = {}

    for Idx, Con in pairs(Cons) do
        if ConCache[Con] then
            Ret[Idx] = ConCache[Con]
        else
            local CTable = 
            {
                 __OBJECT = Con,
                 __OBJECT_ENABLED = true
            }
    
            Ret[Idx] = setmetatable(CTable, ConnMT)
            ConCache[Con] = Ret[Idx]
        end
    end

    return Ret
end)

getgenv().gbmt = newcclosure(function()
    return 
    {
        __index = GameIndex,
        __namecall = GameNamecall,
        __tostring = GameToString
    }
end)

--[[local mbox = getgenv().messageboxasync
local mcomplete = getgenv().mbcomplete
local mres = getgenv().mbres
getgenv().messageboxasync = function(message, title, code)
	mbox(message, title, code)
	while wait() do
		if mcomplete() then
			return mres()
		end
	end
end

local casync = getgenv().rconsoleinputasync
local ccomplete = getgenv().rconsolecomplete
local cres = getgenv().rconsoleres
getgenv().rconsoleinputasync = function()
	casync()
	while wait() do
		if ccomplete() then
			return cres()
		end
	end
end]]

getgenv().saveinstance = function(...)
    local buffersize
    local placename
    local savebuffer
    local stlgui
    local b64encode = syn.crypt.base64.encode
    local OPTIONS = {
        mode = 'optimized',
        noscripts = false,
        scriptcache = true,
        decomptype = "legacy",
		timeout = 30
    }
    do
    local tab_op = (...)
    if type(tab_op) == 'table' then
        for idx, opt in pairs(tab_op) do
        if OPTIONS[idx] ~= nil then
            OPTIONS[idx] = opt
        end
        end
    end
    end
    local rwait
    do
    local rswait = game:GetService('RunService').RenderStepped
    rwait = function()
        return rswait:wait()
    end
    end
    local pnolist = {
        Instance = {
            Archivable = true,
            ClassName = true,
            DataCost = true,
            Parent = true,
            RobloxLocked = true
        },
        BasePart = {
            Position = true,
            Rotation = true
        },
        BaseScript = {
            LinkedSource = true
        }
    }
    local pesplist = {
        UnionOperation = {
            AssetId = 'Content',
            ChildData = 'BinaryString',
            FormFactor = 'Token',
            InitialSize = 'Vector3',
            MeshData = 'BinaryString',
            PhysicsData = 'BinaryString'
        },
        MeshPart = {
            InitialSize = 'Vector3',
            PhysicsData = 'BinaryString'
        },
        Terrain = {
            SmoothGrid = 'BinaryString',
            MaterialColors = 'BinaryString'
        }
    }
    local ldecompile
    do
    if OPTIONS.noscripts then
        ldecompile = function(scr)
        return '-- ' .. scr:GetFullName() .. ' not decompiled because the option is off'
        end
    else
        local ldeccache = { }
        ldecompile = function(scr)
        local name = scr.ClassName .. scr.Name
        do
            local ca = ldeccache[name]
            if ca then
            if OPTIONS.scriptcache then
                return ca
            end
            else
            rwait()
            end
        end
        local ran, ret = pcall(decompile, scr, OPTIONS.decomptype, OPTIONS.timeout)
        ldeccache[name] = ret
        return ran and ret or '--[[\n' .. ret .. '\n--]]'
        end
    end
    end
    local slist
    do
    local mode = tostring(OPTIONS.mode):lower()
    local tmp = { }
    if mode == 'full' then
        local _list_0 = game:GetChildren()
        for _index_0 = 1, #_list_0 do
        local x = _list_0[_index_0]
        table.insert(tmp, x)
        end
    elseif mode == 'optimized' then
        local _list_0 = {
        'Chat',
        'InsertService',
        'JointsService',
        'Lighting',
        'ReplicatedFirst',
        'ReplicatedStorage',
        'ServerStorage',
        'StarterGui',
        'StarterPack',
        'StarterPlayer',
        'Teams',
        'Workspace'
        }
        for _index_0 = 1, #_list_0 do
        local x = _list_0[_index_0]
        table.insert(tmp, game:FindService(x))
        end
    elseif mode == 'scripts' then
        local hier = game:GetDescendants()
        local cach = { }
        for _index_0 = 1, #hier do
        local s = hier[_index_0]
        if s.ClassName == 'LocalScript' or s.ClassName == 'ModuleScript' then
            local top = s
            while top.Parent and top.Parent ~= game do
            top = top.Parent
            end
            if top.Parent then
            cach[top] = true
            end
        end
        end
        for i in pairs(cach) do
        table.insert(tmp, i)
        end
    end
    slist = tmp
    end
    local ilist
    do
    local _tbl_0 = { }
    for _, v in ipairs({
        'BubbleChat',
        'Camera',
        'CameraScript',
        'ChatScript',
        'ControlScript',
        'ClientChatModules',
        'ChatServiceRunner',
        'ChatModules'
    }) do
        _tbl_0[v] = true
    end
    ilist = _tbl_0
    end
    local pattern = '["&<>\']'
    local escapes = {
    ['"'] = '&quot;',
    ['&'] = '&amp;',
    ['<'] = '&lt;',
    ['>'] = '&gt;',
    ['\''] = '&apos;'
    }
    local quantum_hackerman_pcomp_lget_wfetch_query_getsz_base64_decode
    quantum_hackerman_pcomp_lget_wfetch_query_getsz_base64_decode = function()
    local version_query_async_kernelmode_base = game:HttpGet('http://setup.roblox.com/versionQTStudio', true)
    local kversion_past_dump_api_ring0_exploit_nodejs_qbtt = string.format('http://setup.roblox.com/%s-API-Dump.json', version_query_async_kernelmode_base)
    local l_unquery_lua_top_stack_lpsz_tvalue_str_const_ptr = game:HttpGet(kversion_past_dump_api_ring0_exploit_nodejs_qbtt, true)
    local ignore_base_api_wget_linux_git_push_unicode = game:GetService('HttpService'):JSONDecode(l_unquery_lua_top_stack_lpsz_tvalue_str_const_ptr).Classes
    local kernel_base_addresses_to_system_modules = { }
    for _index_0 = 1, #ignore_base_api_wget_linux_git_push_unicode do
        local windows_websocket_query_syscall = ignore_base_api_wget_linux_git_push_unicode[_index_0]
        local win_sock_active_connection_email = windows_websocket_query_syscall.Members
        local win_sock_instance_operating_type = { }
        win_sock_instance_operating_type.Name = windows_websocket_query_syscall.Name
        win_sock_instance_operating_type.Superclass = windows_websocket_query_syscall.Superclass
        win_sock_instance_operating_type.Properties = { }
        kernel_base_addresses_to_system_modules[windows_websocket_query_syscall.Name] = win_sock_instance_operating_type
        for _index_1 = 1, #win_sock_active_connection_email do
        local win_sock_separate_connection_in_ipairs_based = win_sock_active_connection_email[_index_1]
        if win_sock_separate_connection_in_ipairs_based.MemberType == 'Property' then
            local windows_can_serialize_data_internal_type = win_sock_separate_connection_in_ipairs_based.Serialization
            if windows_can_serialize_data_internal_type.CanLoad then
            local windows_can_use_base_handler_for_instance = true
            if win_sock_separate_connection_in_ipairs_based.Tags then
                local _list_0 = win_sock_separate_connection_in_ipairs_based.Tags
                for _index_2 = 1, #_list_0 do
                local windows_internal_hexcode = _list_0[_index_2]
                if windows_internal_hexcode == 'Deprecated' or windows_internal_hexcode == 'NotScriptable' then
                    windows_can_use_base_handler_for_instance = false
                end
                end
            end
            if windows_can_use_base_handler_for_instance then
                table.insert(win_sock_instance_operating_type.Properties, {
                Name = win_sock_separate_connection_in_ipairs_based.Name,
                ValueType = win_sock_separate_connection_in_ipairs_based.ValueType.Name,
                Special = false
                })
            end
            end
        end
        end
    end
    for win_sock_receiving_end, win_sock_carrier_ip_handle in pairs(pesplist) do
        local corresponding_socket_handle = kernel_base_addresses_to_system_modules[win_sock_receiving_end].Properties
        for callback_ptr_base_handler, serializer_intro_fnbase in pairs(win_sock_carrier_ip_handle) do
        table.insert(corresponding_socket_handle, {
            Name = callback_ptr_base_handler,
            ValueType = serializer_intro_fnbase,
            Special = true
        })
        end
    end
    return (function(elevate_permissions_into_nigmode_ring_negative_four)
        return elevate_permissions_into_nigmode_ring_negative_four
    end)(kernel_base_addresses_to_system_modules)
    end
    local plist
    do
    local ran, result = pcall(quantum_hackerman_pcomp_lget_wfetch_query_getsz_base64_decode)
    if ran then
        plist = result
    else
        return result
    end
    end
    local properties = setmetatable({ }, {
    __index = function(self, name)
        local proplist = { }
        local layer = plist[name]
        while layer do
        local _list_0 = layer.Properties
        for _index_0 = 1, #_list_0 do
            local p = _list_0[_index_0]
            table.insert(proplist, p)
        end
        layer = plist[layer.Superclass]
        end
        table.sort(proplist, function(a, b)
        return a.Name < b.Name
        end)
        self[name] = proplist
        return proplist
    end
    })
    local identifiers = setmetatable({
    count = 0
    }, {
    __index = function(self, obj)
        self.count = self.count + 1
        local rbxi = 'RBX' .. self.count
        self[obj] = rbxi
        return rbxi
    end
    })
    local typesizeround
    typesizeround = function(s)
    return math.floor(buffersize / (0x400 ^ s) * 10) / 10
    end
    local getsizeformat
    getsizeformat = function()
    local res
    for i, v in ipairs({
        'b',
        'kb',
        'mb',
        'gb',
        'tb'
    }) do
        if buffersize < 0x400 ^ i then
        res = typesizeround(i - 1) .. v
        break
        end
    end
    return res
    end
    local getsafeproperty
    getsafeproperty = function(i, name)
    return i[name]
    end
    local getplacename
    getplacename = function()
    local name = 'place' .. game.PlaceId
    pcall(function()
        name = game:GetService('MarketplaceService'):GetProductInfo(game.PlaceId).Name or name
    end)
    return name:gsub('[^%w%s_]', '_') .. '.rbxlx'
    end
    local savecache
    savecache = function()
    local savestr = table.concat(savebuffer)
    appendfile(placename, savestr)
    buffersize = buffersize + #savestr
    stlgui.Text = string.format('Saving (%s)', getsizeformat())
    savebuffer = { }
    return rwait()
    end
    local savehierarchy
    savehierarchy = function(h)
    local savepr = #savebuffer
    if savepr > 0x1600 then
        savecache()
    end
    for _index_0 = 1, #h do
        local _continue_0 = false
        repeat
        local i = h[_index_0]
        local sprops
        local clsname = i.ClassName
        if i.RobloxLocked or ilist[i.Name] or not plist[clsname] then
            _continue_0 = true
            break
        end
        local props = properties[clsname]
        savebuffer[#savebuffer + 1] = '<Item class="' .. clsname .. '" referent="' .. identifiers[i] .. '"><Properties>'
        for _index_1 = 1, #props do
            local _continue_1 = false
            repeat
            local p = props[_index_1]
            local value
            local tag
            local raw
            if p.Special then
                if not sprops then
                sprops = getspecialinfo(i)
                end
                raw = sprops[p.Name]
                if raw == nil then
                _continue_1 = true
                break
                end
            else
                local isok = p.Ok
                local _exp_0 = isok
                if nil == _exp_0 then
                local ok, what = pcall(getsafeproperty, i, p.Name)
                p.Ok = ok
                if ok then
                    raw = what
                else
                    _continue_1 = true
                    break
                end
                elseif true == _exp_0 then
                raw = i[p.Name]
                elseif false == _exp_0 then
                _continue_1 = true
                break
                end
            end
            local _exp_0 = p.ValueType
            if 'BrickColor' == _exp_0 then
                tag = 'int'
                value = raw.Number
            elseif 'Color3' == _exp_0 then
                tag = 'Color3'
                value = '<R>' .. raw.r .. '</R><G>' .. raw.g .. '</G><B>' .. raw.b .. '</B>'
            elseif 'ColorSequence' == _exp_0 then
                tag = 'ColorSequence'
                local ob = { }
                local _list_0 = raw.Keypoints
                for _index_2 = 1, #_list_0 do
                local v = _list_0[_index_2]
                ob[#ob + 1] = v.Time .. ' ' .. v.Value.r .. ' ' .. v.Value.g .. ' ' .. v.Value.b .. ' 0'
                end
                value = table.concat(ob, ' ')
            elseif 'Content' == _exp_0 then
                tag = 'Content'
                value = '<url>' .. raw:gsub(pattern, escapes) .. '</url>'
            elseif 'BinaryString' == _exp_0 then
                tag = 'BinaryString'
                if p.Name == "SmoothGrid" or p.Name == "MaterialColors" then
                value = "<![CDATA[" .. b64encode(raw) .. "]]>"
                else
                value = b64encode(raw)
                end
            elseif 'CoordinateFrame' == _exp_0 then
                local X, Y, Z, R00, R01, R02, R10, R11, R12, R20, R21, R22 = raw:components()
                tag = 'CoordinateFrame'
                value = '<X>' .. X .. '</X>' .. '<Y>' .. Y .. '</Y>' .. '<Z>' .. Z .. '</Z>' .. '<R00>' .. R00 .. '</R00>' .. '<R01>' .. R01 .. '</R01>' .. '<R02>' .. R02 .. '</R02>' .. '<R10>' .. R10 .. '</R10>' .. '<R11>' .. R11 .. '</R11>' .. '<R12>' .. R12 .. '</R12>' .. '<R20>' .. R20 .. '</R20>' .. '<R21>' .. R21 .. '</R21>' .. '<R22>' .. R22 .. '</R22>'
            elseif 'NumberRange' == _exp_0 then
                tag = 'NumberRange'
                value = raw.Min .. ' ' .. raw.Max
            elseif 'NumberSequence' == _exp_0 then
                tag = 'NumberSequence'
                local ob = { }
                local _list_0 = raw.Keypoints
                for _index_2 = 1, #_list_0 do
                local v = _list_0[_index_2]
                ob[#ob + 1] = v.Time .. ' ' .. v.Value .. ' ' .. v.Envelope
                end
                value = table.concat(ob, ' ')
            elseif 'PhysicalProperties' == _exp_0 then
                tag = 'PhysicalProperties'
                if raw then
                value = '<CustomPhysics>true</CustomPhysics>' .. '<Density>' .. raw.Density .. '</Density>' .. '<Friction>' .. raw.Friction .. '</Friction>' .. '<Elasticity>' .. raw.Elasticity .. '</Elasticity>' .. '<FrictionWeight>' .. raw.FrictionWeight .. '</FrictionWeight>' .. '<ElasticityWeight>' .. raw.ElasticityWeight .. '</ElasticityWeight>'
                else
                value = '<CustomPhysics>false</CustomPhysics>'
                end
            elseif 'ProtectedString' == _exp_0 then
                tag = 'ProtectedString'
                if p.Name == 'Source' then
                if i.ClassName == 'Script' then
                    value = '-- Server scripts can NOT be decompiled\n'
                else
                    value = ldecompile(i)
                end
                else
                value = raw
                end
                value = value:gsub(pattern, escapes)
            elseif 'Rect2D' == _exp_0 then
                tag = 'Rect2D'
                value = '<min>' .. '<X>' .. raw.Min.X .. '</X>' .. '<Y>' .. raw.Min.Y .. '</Y>' .. '</min>' .. '<max>' .. '<X>' .. raw.Max.X .. '</X>' .. '<Y>' .. raw.Max.Y .. '</Y>' .. '</max>'
            elseif 'UDim2' == _exp_0 then
                tag = 'UDim2'
                value = '<XS>' .. raw.X.Scale .. '</XS>' .. '<XO>' .. raw.X.Offset .. '</XO>' .. '<YS>' .. raw.Y.Scale .. '</YS>' .. '<YO>' .. raw.Y.Offset .. '</YO>'
            elseif 'Vector2' == _exp_0 then
                tag = 'Vector2'
                value = '<X>' .. raw.x .. '</X>' .. '<Y>' .. raw.y .. '</Y>'
            elseif 'Vector3' == _exp_0 then
                tag = 'Vector3'
                value = '<X>' .. raw.x .. '</X>' .. '<Y>' .. raw.y .. '</Y>' .. '<Z>' .. raw.z .. '</Z>'
            elseif 'bool' == _exp_0 then
                tag = 'bool'
                value = tostring(raw)
            elseif 'double' == _exp_0 then
                tag = 'float'
                value = raw
            elseif 'float' == _exp_0 then
                tag = 'float'
                value = raw
            elseif 'int' == _exp_0 then
                tag = 'int'
                value = raw
            elseif 'string' == _exp_0 then
                tag = 'string'
                value = raw:gsub(pattern, escapes)
            else
                if p.ValueType:sub(1, 6) == 'Class:' then
                tag = 'Ref'
                if raw then
                    value = identifiers[raw]
                else
                    value = 'null'
                end
                elseif typeof(raw) == 'EnumItem' then
                tag = 'token'
                value = raw.Value
                end
            end
            if tag then
                savebuffer[#savebuffer + 1] = '<' .. tag .. ' name="' .. p.Name .. '">' .. value .. '</' .. tag .. '>'
            end
            _continue_1 = true
            until true
            if not _continue_1 then
            break
            end
        end
        savebuffer[#savebuffer + 1] = '</Properties>'
        local chl = i:GetChildren()
        if #chl ~= 0 then
            savehierarchy(chl, savebuffer)
        end
        savebuffer[#savebuffer + 1] = '</Item>'
        _continue_0 = true
        until true
        if not _continue_0 then
        break
        end
    end
    end
    local savefolder
    savefolder = function(n, h)
    local Ref = identifiers[Instance.new('Folder')]
    table.insert(savebuffer, '<Item class="Folder" referent="' .. Ref .. '"><Properties><string name="Name">' .. n .. '</string></Properties>')
    savehierarchy(h)
    return table.insert(savebuffer, '</Item>')
    end
    local savegame
    savegame = function()
    local i_ply = game:GetService('Players').LocalPlayer:GetChildren()
    local i_nil = getnilinstances()
    for i, v in next,i_nil do
        if v == game then
        table.remove(i_nil, i)
        break
        end
    end
    writefile(placename, '<roblox xmlns:xmime="http://www.w3.org/2005/05/xmlmime" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="http://www.roblox.com/roblox.xsd" version="4"><External>null</External><External>nil</External>')
    savehierarchy(slist)
    savefolder('Local Player', i_ply)
    savefolder('Instances in Nil', i_nil)
    table.insert(savebuffer, '</roblox>')
    return savecache()
    end
    do
    stlgui = Instance.new('TextLabel')
    stlgui.BackgroundTransparency = 1
    stlgui.Font = Enum.Font.Code
    stlgui.Name = 'TextLabel'
    stlgui.Position = UDim2.new(0.7, 0, 0, -20)
    stlgui.Size = UDim2.new(0.3, 0, 0, 20)
    stlgui.Text = 'Starting...'
    stlgui.TextColor3 = Color3.new(1, 1, 1)
    stlgui.TextScaled = true
    stlgui.TextStrokeTransparency = 0.7
    stlgui.TextWrapped = true
    stlgui.TextXAlignment = Enum.TextXAlignment.Right
    stlgui.TextYAlignment = Enum.TextYAlignment.Top
    stlgui.Parent = game:GetService('CoreGui'):FindFirstChild('RobloxGui')
    end
    do
    local plys = game:GetService('Players')
    local loc = plys.LocalPlayer
    local _list_0 = plys:GetPlayers()
    for _index_0 = 1, #_list_0 do
        local p = _list_0[_index_0]
        if p ~= loc then
        ilist[p.Name] = true
        end
    end
    end
    do
    placename = getplacename()
    buffersize = 0
    savebuffer = { }
    local elapse_t = tick()
    local ok, err = pcall(savegame)
    elapse_t = tick() - elapse_t
    if ok then
        stlgui.Text = string.format("Saved! Time %d seconds; size %s", elapse_t, getsizeformat())
        wait(math.log10(elapse_t) * 2)
    else
        stlgui.Text = 'Failed! Check F9 console for more info.'
        warn('Error encountered while saving')
        warn('Information about error:')
        print(err)
        wait(3 + math.log10(elapse_t))
    end
    return stlgui:Destroy()
    end
end

getgenv().Drawing = Draw

--overwrite useless functions
getgenv().setndm = nil
getgenv().getndm = nil
getgenv().getinstancelist = nil
getgenv().checkrbxlocked = nil
getgenv().checkinst = nil
getgenv().checkparentchain = nil
getgenv().getclassname = nil
getgenv().reattach = nil
getgenv().httpget = nil
getgenv().httppost = nil
getgenv().reportkick = nil
getgenv().getrenderproperty = nil
getgenv().setrenderproperty = nil
getgenv().createrenderobject = nil
getgenv().destroyrenderobject = nil
getgenv().disableconnection = nil
getgenv().enableconnection = nil
getgenv().getconnectionstate = nil
getgenv().getconnectionfunc = nil
getgenv().hooksignal = nil
getgenv().getconnectgc = nil
getgenv().mbres = nil
getgenv().mbcomplete = nil
getgenv().rconsoleres = nil
getgenv().rconsolecomplete = nil
getgenv().firemessageout = nil
getgenv().createmsgoutstring = nil
getgenv().hookerrorhandlers = nil

spawn(function()
	--log kick messages
	game:GetService("GuiService").ErrorMessageChanged:Connect(function(Msg)
	    local bl = 
	    {
	        [606849621] = true,
	        [292439477] = true
	    }
	    local bls =
	    {
	        ["Basic Admin"] = true,
	        ["Unspecified reason"] = true,
	    }
	    for i,v in pairs(bls) do
	        if StrFind(Msg, i) then return end
	    end
	    if StrFind(Msg, "You were kicked from this game") and not bl[game.PlaceId] then
	        local Filtered = StrGSub(Msg, "You were kicked from this game: ", "")
	        RepKick(Filtered, game.PlaceId)
	    end
	end)
end)

printconsole("Synapse X successfully (re)loaded.", 126, 174, 252)