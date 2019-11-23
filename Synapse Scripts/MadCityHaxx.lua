--[[
    __  ___          _________ __        __  __               
   /  |/  /___ _____/ / ____(_) /___  __/ / / /___ __  ___  __
  / /|_/ / __ `/ __  / /   / / __/ / / / /_/ / __ `/ |/_/ |/_/
 / /  / / /_/ / /_/ / /___/ / /_/ /_/ / __  / /_/ />  <_>  <  
/_/  /_/\__,_/\__,_/\____/_/\__/\__, /_/ /_/\__,_/_/|_/_/|_|  
                               /____/                         v1.0

							By 3dsboy08
                                                                   
]]

do
	if game.PlaceId ~= 1224212277 then return end
	
	local function trick() return 'asd' end
	if not is_synapse_function(trick) then while true do end end
	
	local LocalPlayer = game:GetService("Players").LocalPlayer
	if not LocalPlayer.Character or LocalPlayer.Character.Parent == nil then
		LocalPlayer.CharacterAdded:Wait()
	end
end

local Utils = {}
do
	local Camera = game:GetService("Workspace").CurrentCamera
	local Players = game:GetService("Players")
	local Player = Players.LocalPlayer
	local Character = Player.Character
	local NewCharEvent = Instance.new("BindableEvent")
	
	function Utils.ConvertTable(Table, KeyValue)
		local Result = {}
		if KeyValue then
			for I,V in pairs(Table) do
				Result[#Result + 1] = 
				{
					["Key"] = I,
					["Value"] = V
				}
			end
		else
			for I,V in pairs(Table) do
				Result[#Result + 1] = V
			end
		end
		return Result
	end
	
	function Utils.GetDistance(From, To)
    	local Point1
   	 	local Point2

   	 	if typeof(From) == 'Vector3' then
	        Point1 = From
	    elseif typeof(From) == 'Instance' and From:IsA'BasePart' then
	        Point1 = From.Position
	    else
	        Point1 = Vector3.new()
	    end
	
	    if typeof(To) == 'Vector3' then
	        Point2 = To
	    elseif typeof(To) == 'Instance' and To:IsA'BasePart' then
	        Point2 = To.Position
	    elseif To == nil then
	        Point2 = Camera.CFrame.p
	    else
	        Point2 = Vector3.new()
	    end
	
	    return (Point1 - Point2).magnitude
	end

	function Utils.GetNearestPlayer(Radius, TeamCheck, Friendly, CrimCheck, Blacklist)
	    if not Radius then Radius = 10000 end
	
	    local Nearest
	    local Closest = Radius
	
	    for i, v in pairs(Players:GetPlayers()) do
	        if v ~= Player and v.Character ~= nil and v.Character:FindFirstChild'HumanoidRootPart' then
	            local ShouldContinue = true
	            if TeamCheck and v.TeamColor == Player.TeamColor then
	                ShouldContinue = false
	            end
				if CrimCheck and tostring(v.Team) ~= "Criminals" then
					ShouldContinue = false
				end
				if Blacklist then
					for I2,V2 in pairs(Blacklist) do
						if I2.Name == v.Name and V2 then
							ShouldContinue = false
						end
					end
				end
	            if Friendly then
	                if v.TeamColor == Player.TeamColor then
	                    ShouldContinue = true
	                else
	                    ShouldContinue = false
	                end
	            end
	            if ShouldContinue then
	                local Distance = Utils.GetDistance(v.Character.HumanoidRootPart)
	                if Distance < Closest then
	                    Closest = Distance
	                    Nearest = v
	                end
	            end
	        end
	    end
	
	    return Nearest
	end
	
	function Utils.GetPlayer()
		return Player
	end
	
	function Utils.GetCharacter()
		return Character
	end
	
	function Utils.NewCharacter(Func)
		NewCharEvent.Event:Connect(Func)
	end
	
	function Utils.Split(str, sep)
   		local result = {}
  		local regex = ("([^%s]+)"):format(sep)
  	 	for each in str:gmatch(regex) do
      		table.insert(result, each)
   		end
   		return result
	end
	
	function Utils.FindUpvalue(name)
		for i,v in pairs(getreg()) do
			if type(v) == "function" and islclosure(v) and not is_synapse_function(v) then
				for i2,v2 in pairs(debug.getupvalues(v)) do
					if i2 == name then return v end
				end
			end
		end
	end
	
	Player.CharacterAdded:Connect(function(NewChar)
		Character = NewChar
		NewCharEvent:Fire(NewChar)
	end)
end

local Remote = {}
do
	local Event = game:GetService("ReplicatedStorage").Event
	
	function Remote.FireServer(...)
		Event:FireServer(...)
	end
end

local Settings = {}
do
	local RealSettings = {}
	local RealSavedSettings = {}
	
	function Settings.Set(Key, Val)
		RealSettings[Key] = Val
	end
	
	function Settings.Get(Key)
		return RealSettings[Key]
	end
	
	function Settings.SetSaved(Key, Val)
		RealSavedSettings[Key] = Val
	end
	
	function Settings.GetSaved(Key)
		return RealSavedSettings[Key]
	end
	
	--Save settings
	local Suc, Res = pcall(function()
		return game:GetService("HttpService"):JSONDecode(readfile("madchaxx_settings.bin"))
	end)
	
	if Suc and type(Res) == "table" then
		for I,V in pairs(Res) do
			Settings.SetSaved(I, V)
		end
	end
	
	if not Settings.GetSaved("WalkSpeed") then
		Settings.SetSaved("WalkSpeed", 50)
	end
	
	if not Settings.GetSaved("JumpPower") then
		Settings.SetSaved("JumpPower", 100)
	end
	
	if not Settings.GetSaved("Active") then
		Settings.SetSaved("Active", {})
	end
		
	if not Settings.GetSaved("KeyBinds") then
		Settings.SetSaved("KeyBinds", {})
	end
	
	spawn(function()
		while wait(10) do
			writefile("madchaxx_settings.bin", game:GetService("HttpService"):JSONEncode(RealSavedSettings))
		end
	end)
end

local Bypass = {}
do
	local CarObject
	local Stats
	local EquipTable = {}
	for i,v in pairs(Utils.GetPlayer().Backpack:GetDescendants()) do
		if v:FindFirstChild("PistolScript") or v:FindFirstChild("RifleScript") or v:FindFirstChild("ItemScript") or v:FindFirstChild("ShotgunScript") or v:FindFirstChild("TazerScript") or v:FindFirstChild("GrenadeScript") or v:FindFirstChild("PowerScript") then
			for i2,v2 in next, v:GetDescendants() do 
				if v2:IsA("LocalScript") then
					EquipTable[v.Name] = v2.Parent:WaitForChild("Handle")
				end 
			end
		end
	end
	for i,v in pairs(Utils.GetCharacter():GetDescendants()) do
		if v:FindFirstChild("PistolScript") or v:FindFirstChild("RifleScript") or v:FindFirstChild("ItemScript") or v:FindFirstChild("ShotgunScript") or v:FindFirstChild("TazerScript") or v:FindFirstChild("GrenadeScript") or v:FindFirstChild("PowerScript") then
			for i2,v2 in next, v:GetDescendants() do 
				if v2:IsA("LocalScript") then
					EquipTable[v.Name] = v2.Parent:WaitForChild("Handle")
				end 
			end
		end
	end
	
	spawn(function()
		 CarObject = game:GetService("Players").LocalPlayer.PlayerGui:WaitForChild("CarChassis"):WaitForChild("Car").Value
		 Stats = require(CarObject.Settings)
	end)
	
	function Bypass.Teleport(...)
		local HRP = Utils.GetCharacter().HumanoidRootPart
		HRP.CFrame = CFrame.new(Vector3.new(...))
		return
	end
	
	function Bypass.WalkSpeed(Disable)
		if Disable then
			Utils.GetCharacter():WaitForChild'Humanoid'.WalkSpeed = 16
		else
			Utils.GetCharacter():WaitForChild'Humanoid'.WalkSpeed = Settings.GetSaved("WalkSpeed")
		end
	end
	
	function Bypass.JumpPower(Disable)
		if Disable then
			Utils.GetCharacter():WaitForChild'Humanoid'.JumpPower = 50
		else
			Utils.GetCharacter():WaitForChild'Humanoid'.JumpPower = Settings.GetSaved("JumpPower")
		end
	end
	
	function Bypass.Damage(Player)
		Bypass.Equip("Pistol")
		Remote.FireServer("SHOTTY", Player.Character.Head.Position)
	end
	
	function Bypass.Taser(Player)
		Bypass.Equip("Tazer")
		Remote.FireServer("TAZ", Player.Character.UpperTorso)
	end
	
	function Bypass.Arrest(Player)
		Bypass.Equip("Handcuffs")
		Remote.FireServer("Arrest", Player)
	end
	
	function Bypass.Equip(Name)
		Remote.FireServer("Equip", Utils.GetPlayer(), EquipTable[Name])
	end
	
	--Do initial TP bypass
	local Hint = Instance.new("Hint")
	local Attempts = 1
	Hint.Text = "Please wait while MadCityHaxx bypasses Mad City's anticheat. This may take a little while... (attempt " .. tostring(Attempts) .. ")"
	Hint.Parent = workspace
	local SGui = Instance.new("ScreenGui")
	local SFrame = Instance.new("Frame", SGui)
	SFrame.Size = UDim2.new(1, 0, 1, 0)
	SFrame.BackgroundColor3 = Color3.fromRGB(0, 0, 0)
	SGui.Parent = game:GetService("CoreGui")
	local Rnd = Random.new()
	local BackupRP = Utils.GetCharacter().HumanoidRootPart
	local BackupCF = BackupRP.CFrame
	local function RunAntiTelportRC()
		for i=0,25 do
			BackupRP.Velocity = Vector3.new(0, 0,0)
			Bypass.Teleport(Rnd:NextNumber(1, 5000), Rnd:NextNumber(1, 5000), Rnd:NextNumber(1, 5000))
			BackupRP.Velocity = Vector3.new(0, 0,0)
			wait()
		end
		for i=0,5 do
			Bypass.Teleport(BackupCF.x, BackupCF.y, BackupCF.z)
			wait()
		end
		wait(6)
		for i=0,5 do
			Bypass.Teleport(BackupCF.x, BackupCF.y, BackupCF.z)
			wait()
		end
	end
	while true do
		RunAntiTelportRC()
		BackupRP.Velocity = Vector3.new(0, 0,0)
		Bypass.Teleport(745.038269, 27.968502, 447.69632)
		BackupRP.Velocity = Vector3.new(0, 0,0)
		wait(1)
		if Utils.GetDistance(Vector3.new(745.038269, 27.968502, 447.69632), Utils.GetCharacter().HumanoidRootPart) < 10 then break end
		Attempts = Attempts + 1
		Hint.Text = "Please wait while MadCityHaxx bypasses Mad City's anticheat. This may take a little while... (attempt " .. tostring(Attempts) .. ")"
		wait()
	end
	for i=0,5 do
		Bypass.Teleport(BackupCF.x, BackupCF.y, BackupCF.z)
		wait()
	end
	SGui:Destroy()
	Hint:Destroy()
	
	function Bypass.SetNitro(Amount)
		if CarObject and Stats then
			CarObject.CarChassis.Boost.Value = Amount
			Stats.Boost = true			
		end
	end
	
	Utils.NewCharacter(function()
		Bypass.WalkSpeed(not Settings.Get("WalkSpeedEnabled"))
		Bypass.JumpPower(not Settings.Get("JumpPowerEnabled"))
	end)
	
	--Metatable hook
	loadstring([[
	local MT = getrawmetatable(game)
	local OldNewIndex = MT.__newindex
	setreadonly(MT, false)
	
	MT.__newindex = newcclosure(function(T, K, V)
		if checkcaller() then return OldNewIndex(T, K, V) end
		
		if K == "WalkSpeed" then return end
		if K == "JumpPower" then return end
				
		return OldNewIndex(T, K, V)
	end)
	
	setreadonly(MT, true)
	]])()
	
	local CurrentlyArresting = false
	local CurrentlyKilling = false
	local ArrestComplete = false
	local ArrestBlacklist = {}
	local TpAuraBlacklist = {}
	
	game:GetService("RunService").Stepped:Connect(function()
		if Settings.Get("InfNitro") then
			Bypass.SetNitro(300)
		end
		
		if Settings.Get("NoClip") then
			Utils.GetCharacter().Humanoid:ChangeState(11)
		end
		
		if Settings.Get("MachinePistolHook") then
			local MPFunc = Settings.Get("MachinePistolFunc")
			local MPUpvals = debug.getupvalues(MPFunc)
			local MPEnv = getfenv(MPFunc)
			if MPUpvals["Equipped"] then
				MPUpvals["RecoilAmount"].Value = Vector3.new(0, 0, 0)
				MPUpvals["WC"].PlaySound(MPUpvals["Char"], 139593133, MPUpvals["Torso"])
				MPUpvals["WC"].ShootGun(MPUpvals["Char"], MPUpvals["Char"], MPEnv["GetMousePoint"](MPUpvals["Crosshair"].AbsolutePosition.X, MPUpvals["Crosshair"].AbsolutePosition.Y), MPUpvals["GunName"], MPUpvals["Damage"], MPUpvals["Spread"])
				spawn(function()
					MPEnv["PlayAnimation"](1228022274)
				end)
			end
		end
		
		if Settings.Get("AutoArrest") and tostring(Utils.GetPlayer().Team) == "Police" then
			local Near = Utils.GetNearestPlayer(10000, true, false, true, ArrestBlacklist)
			if Near and not CurrentlyArresting then
				CurrentlyArresting = true
				spawn(function()
					ArrestBlacklist[Near] = true
					local Event
					Event = Near:GetPropertyChangedSignal("Team"):Connect(function()
						ArrestBlacklist[Near] = false
						Event:Disconnect()
					end)
					local Pos = Near.Character.HumanoidRootPart.CFrame.p
					Bypass.Teleport(Pos.x, Pos.y, Pos.z)
					wait(0.2)
					while tostring(Near.Team) == "Criminals" and not ArrestComplete and Settings.Get("AutoArrest") do
						Bypass.Taser(Near)
						Bypass.Arrest(Near)
						Utils.GetCharacter().HumanoidRootPart.CFrame = Near.Character.HumanoidRootPart.CFrame - Vector3.new(3, 0, 0)
						wait()
					end
					CurrentlyArresting = false
					ArrestComplete = false
				end)
			end
		end
		
		if Settings.Get("TpAura") then
			local Near = Utils.GetNearestPlayer(10000, true, false, true, TpAuraBlacklist)
			if Near and not CurrentlyKilling then
				CurrentlyKilling = true
				spawn(function()
					TpAuraBlacklist[Near] = true
					local Event
					Event = Near.CharacterAdded:Connect(function()
						TpAuraBlacklist[Near] = false
						Event:Disconnect()
					end)
					local Pos = Near.Character.HumanoidRootPart.CFrame.p
					Bypass.Teleport(Pos.x, Pos.y, Pos.z)
					wait(0.2)
					while Near.Character.Humanoid.Health ~= 0 and Settings.Get("TpAura") do
						Bypass.Damage(Near)
						Utils.GetCharacter().HumanoidRootPart.CFrame = Near.Character.HumanoidRootPart.CFrame - Vector3.new(3, 0, 0)
						wait()
					end
					CurrentlyKilling = false	
				end)
			end
		end
		
		if Settings.Get("KillAura") then
			local Near = Utils.GetNearestPlayer(100, true, false, true)
			if Near then
				Bypass.Damage(Near)
			end
		end
	end)
	
	local Mouse = Utils.GetPlayer():GetMouse()
		
	Mouse.Button1Down:connect(function()
		if Settings.Get("ClickTeleport") then
			local V3 = Mouse.Hit.p + Vector3.new(0, 2, 0)
			Bypass.Teleport(V3.x, V3.y, V3.z)
		end
	end)
end

local UI = {}
do
	local Render = loadstring(game:HttpGet("https://cdn.synapse.to/synapsedistro/hub/HaxxMenuMadCity.lua", true))()
	loadstring(game:HttpGet("https://cdn.synapse.to/synapsedistro/hub/ESPLib.lua", true))()
	local ESPLib = shared.uESP
	ESPLib.Enabled = false
	ESPLib.Settings.DrawTracers = false

	local RenderSettings =
	{
	    ['Theme'] = 
	    {
			['Main'] = Color3.fromRGB(171, 71, 188),
			['Background'] = Color3.fromRGB(0, 0, 0),
			['TextColor'] = Color3.fromRGB(255, 255, 255)
		},
		['WindowCount'] = -1,
		['Draggable'] = true,
		['Keybind'] = Enum.KeyCode.RightShift
	}
	
	local Menu = Render.CreateMenu(RenderSettings)
	local MenuOptions = Menu.MenuOptions
	
	local CombatMenu = MenuOptions.CreateWindow("Combat")
	local MovementMenu = MenuOptions.CreateWindow("Movement")
	local RenderMenu = MenuOptions.CreateWindow("Render")
	local TeleportsMenu = MenuOptions.CreateWindow("Teleports")
	local UtilityMenu = MenuOptions.CreateWindow("Utility")
	local SettingsMenu = MenuOptions.CreateWindow("Settings")
	
	local TpAura = CombatMenu.Add("toggle", "TpAura")
	local KillAura = CombatMenu.Add("toggle", "KillAura")
	local AutoArrest = CombatMenu.Add("toggle", "Auto Arrest")
	local InfAmmo = CombatMenu.Add("clickable", "Infinite Ammo")
	local MachinePistol = CombatMenu.Add("clickable", "Machine Pistol")
	
	local Speed = MovementMenu.Add("toggle", "Speed")
	local SuperJump = MovementMenu.Add("toggle", "Super Jump")
	local NoClip = MovementMenu.Add("toggle", "NoClip")
	local ClickTeleport = MovementMenu.Add("toggle", "Click Teleport")
	
	local ESP = RenderMenu.Add("toggle", "ESP")
	local Tracers = RenderMenu.Add("toggle", "Tracers")
	
	local Dock = TeleportsMenu.Add("clickable", "Dock")
	local Garage = TeleportsMenu.Add("clickable", "Garage")
	local Bank = TeleportsMenu.Add("clickable", "Bank")
	local CoffeeShop = TeleportsMenu.Add("clickable", "Coffee Shop")
	local Casino = TeleportsMenu.Add("clickable", "Casino")
	local CrimBase = TeleportsMenu.Add("clickable", "Criminal Base")
	local Jewelry = TeleportsMenu.Add("clickable", "Jewelry Store")
	local HeliPad = TeleportsMenu.Add("clickable", "Helicopter Pad")
	local GunStore = TeleportsMenu.Add("clickable", "Gun Store")
	local HeroBase = TeleportsMenu.Add("clickable", "Hero Base")
	local AirField = TeleportsMenu.Add("clickable", "Air Field")
	local OutsidePrison = TeleportsMenu.Add("clickable", "Outside Prison")
	local InPrison = TeleportsMenu.Add("clickable", "In Prison")
	local PrisonField = TeleportsMenu.Add("clickable", "Prison Field")
	
	local InfNitro = UtilityMenu.Add("toggle", "Infinite Nitro")
	
	local SpeedPlus = SettingsMenu.Add("clickable", "Speed +")
	local SpeedMinus = SettingsMenu.Add("clickable", "Speed -")
	local SuperJumpPlus = SettingsMenu.Add("clickable", "Super Jump +")
	local SuperJumpMinus = SettingsMenu.Add("clickable", "Super Jump -")
	local DisableBlur = SettingsMenu.Add("toggle", "Disable Blur")
	
	Dock.Callback = function()
		Bypass.Teleport(-92.5417404, 27.483942, 274.167969 )
	end
	
	Garage.Callback = function()
	    Bypass.Teleport(242.900726, 27.8274384, -484.288391 )
	end
	
	Bank.Callback = function()
	    Bypass.Teleport(745.038269, 27.968502, 447.69632 )
	end
	
	CoffeeShop.Callback = function()
	    Bypass.Teleport(645.106873, 38.6579895, -95.3533325 )
	end
	
	Casino.Callback = function()
	    Bypass.Teleport(1782.25916, 28.0203667, 682.395386 )
	end
	
	CrimBase.Callback = function()
	    Bypass.Teleport(2032.93066, 27.9056149, 307.196838 )
	end
	
	Jewelry.Callback = function()
	    Bypass.Teleport(-201.371063, 27.8703308, 714.134644 )
	end
	
	HeliPad.Callback = function()
	    Bypass.Teleport(-343.052734, 80.5529633, -278.809296 )
	end
	
	GunStore.Callback = function()
	    Bypass.Teleport(-1612.10583, 45.095211, 681.565125 )
	end
	
	HeroBase.Callback = function()
	    Bypass.Teleport(-1709.20862, 12.6549339, 1525.65527 )
	end
	
	AirField.Callback = function()
	    Bypass.Teleport(-2120.67017, 31.6405296, -1178.73206 )
	end
	
	OutsidePrison.Callback = function()
	    Bypass.Teleport(-893.863037, 56.0996323, -2633.10229 )
	end
	
	InPrison.Callback = function()
	    Bypass.Teleport(-902.866577, 56.5785141, -2893.0105 )
	end
	
	PrisonField.Callback = function()
		Bypass.Teleport(-1005.1217, 54.3845444, -3084.28662)
	end
	
	InfNitro.Callback = function(Type, Name, Value)
		Settings.Set("InfNitro", Value)
	end
	
	DisableBlur.Callback = function(Type, Name, Value)
		Menu.SetBlur(not Value)
	end
	
	Speed.Callback = function(Type, Name, Value)
		Bypass.WalkSpeed(not Value)
		Settings.Set("WalkSpeedEnabled", Value)
	end
	
	SuperJump.Callback = function(Type, Name, Value)
		Bypass.JumpPower(not Value)
		Settings.Set("JumpPowerEnabled", Value)
	end
	
	NoClip.Callback = function(Type, Name, Value)
		Settings.Set("NoClip", Value)
	end
	
	ClickTeleport.Callback = function(Type, Name, Value)
		Settings.Set("ClickTeleport", Value)
	end
	
	AutoArrest.Callback = function(Type, Name, Value)
		Settings.Set("AutoArrest", Value)
	end
	
	TpAura.Callback = function(Type, Name, Value)
		Settings.Set("TpAura", Value)
	end
	
	KillAura.Callback = function(Type, Name, Value)
		Settings.Set("KillAura", Value)
	end
	
	ESP.Callback = function(Type, Name, Value)
		ESPLib.Enabled = Value
	end
	
	Tracers.Callback = function(Type, Name, Value)
		ESPLib.Settings.DrawTracers = Value
	end
	
	InfAmmo.Callback = function()
		for i,v in pairs(Utils.GetPlayer().Backpack:GetDescendants()) do
			if v:FindFirstChild("PistolScript") or v:FindFirstChild("RifleScript") or v:FindFirstChild("ShotgunScript") or v:FindFirstChild("TazerScript") or v:FindFirstChild("GrenadeScript") or v:FindFirstChild("PowerScript") then
				for i2,v2 in next, v:GetDescendants() do 
					if v2:IsA("LocalScript") then
						local env = getsenv(v2)
						debug.setupvalue(env.Reload, "Ammo", math.huge)
						debug.setupvalue(env.Reload, "Clip", math.huge)
						debug.setupvalue(env.Reload, "Recoil", 0)
					end 
				end
			end
		end
	end
	
	local function FindPistolFunc()
		for i,v in pairs(getreg()) do 
			if type(v) == "function" and islclosure(v) and not is_synapse_function(v)  then 
				for i2,v2 in pairs(debug.getupvalues(v)) do 
					if i2 == "GunName" and v2 == "Pistol" then
						local trigger = true
						for i3,v3 in pairs(debug.getconstants(v)) do
							if v3 == "Enum" or v3 == "TargetFilter" then trigger = false end
						end
						if trigger then return v end
					end
				end
			end
		end
	end
	
	local MachinePistolDebounce = false
	MachinePistol.Callback = function()
		if MachinePistolDebounce then return end
		local Func = FindPistolFunc()
		print(Func)
		if not Func then return end
		for i,v in pairs(Utils.GetPlayer().Backpack:GetDescendants()) do
			if v:FindFirstChild("PistolScript") then
				for i2,v2 in next, v:GetDescendants() do 
					if v2:IsA("LocalScript") then
						local env = getsenv(v2)
						debug.setupvalue(env.Reload, "FireRate", math.huge)
						debug.setupvalue(env.Reload, "Spread", math.huge)
						Settings.Set("MachinePistolFunc", Func)
						local Mouse = Utils.GetPlayer():GetMouse()
						local HookEnabled = false
						Mouse.Button1Down:connect(function()
							Settings.Set("MachinePistolHook", true)
						end)
						Mouse.Button1Up:connect(function()
							Settings.Set("MachinePistolHook", false)
						end)
						MachinePistolDebounce = true
					end 
				end
			end
		end
	end
	
	SpeedPlus.Callback = function()
		Settings.SetSaved("WalkSpeed", Settings.GetSaved("WalkSpeed") + 20)
		if Settings.Get("WalkSpeedEnabled") then
			Bypass.WalkSpeed()
		end
	end
	
	SpeedMinus.Callback = function()
		if (0 > Settings.GetSaved("WalkSpeed") - 20) then return end
		Settings.SetSaved("WalkSpeed", Settings.GetSaved("WalkSpeed") - 20)
		if Settings.Get("WalkSpeedEnabled") then
			Bypass.WalkSpeed()
		end
	end
	
	SuperJumpPlus.Callback = function()
		Settings.SetSaved("JumpPower", Settings.GetSaved("JumpPower") + 20)
		if Settings.Get("JumpPowerEnabled") then
			Bypass.JumpPower()
		end
	end
	
	SuperJumpMinus.Callback = function()
		if (0 > Settings.GetSaved("JumpPower") - 20) then return end
		Settings.SetSaved("JumpPower", Settings.GetSaved("JumpPower") - 20)
		if Settings.Get("JumpPowerEnabled") then
			Bypass.JumpPower()
		end
	end
	
	for I,V in pairs(Settings.GetSaved("Active")) do
		if V then
			Menu.EmulateToggle(I)
		end
	end
	
	for I,V in pairs(Settings.GetSaved("KeyBinds")) do
		Menu.EmulateKeyBind(I, Enum.KeyCode[V])
	end
	
	spawn(function()
		while wait(5) do
			local RealKeyBinds = {}
			for I,V in pairs(Menu.GetKeyBinds()) do
				RealKeyBinds[I] = V["Key"].Name
			end
			Settings.SetSaved("KeyBinds", RealKeyBinds)
			Settings.SetSaved("Active", Menu.GetActive())
		end
	end)
end