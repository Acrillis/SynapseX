--[[
	   __      _ ____                    __   __                   
      / /___ _(_) / /_  ________  ____ _/ /__/ /_  ____ __  ___  __
 __  / / __ `/ / / __ \/ ___/ _ \/ __ `/ //_/ __ \/ __ `/ |/_/ |/_/
/ /_/ / /_/ / / / /_/ / /  /  __/ /_/ / ,< / / / / /_/ />  <_>  <  
\____/\__,_/_/_/_.___/_/   \___/\__,_/_/|_/_/ /_/\__,_/_/|_/_/|_|  v5.0-beta

							By 3dsboy08
                                                                   
]]

do
	if game.PlaceId ~= 606849621 then return end
	
	IB_MAX_CFLOW_START()
	local function trick() return 'asd' end
	if not is_synapse_function(trick) then while true do end end
	IB_MAX_CFLOW_END()
	
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
				if CrimCheck and tostring(v.Team) == "Prisoner" then
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
	
	function Utils.ResolveVehicle(Player)
		local Name = Player.Name
		for I,V in pairs(game:GetService("Workspace").Vehicles:GetChildren()) do
			local Seat = V:FindFirstChild("Seat")
			if Seat then
				local PlayerName = Seat:FindFirstChild("PlayerName")
				if PlayerName and PlayerName.Value == Name then return V end				
			end
		end
		return nil
	end
	
	function Utils.Split(str, sep)
		local result = {}
		local regex = ("([^%s]+)"):format(sep)
		for each in str:gmatch(regex) do
			table.insert(result, each)
		end
		return result
	end
	
	Player.CharacterAdded:Connect(function(NewChar)
		Character = NewChar
		NewCharEvent:Fire(NewChar)
	end)
end
	
local AutoUpdate = {}
do
	AutoUpdate.Keys = {}
	
	local PreKeys =
	{
		["[STR_ENCRYPT]Main"] =
		{
			["[STR_ENCRYPT]Arrest"] = 5,
			["[STR_ENCRYPT]Eject"] = 9,
			["[STR_ENCRYPT]Teleport"] = 24
		},
		
		["[STR_ENCRYPT]Bullet"] =
		{
			["[STR_ENCRYPT]Damage"] = 2
		},
		
		["[STR_ENCRYPT]Basic"] =
		{
			["[STR_ENCRYPT]Taser"] = 3
		},
		
		["[STR_ENCRYPT]ItemSystem"] =
		{
			["[STR_ENCRYPT]Bullet"] = 3
		},

		["[STR_ENCRYPT]Paraglide"] =
		{
			["[STR_ENCRYPT]Parachute"] = 2,
			["[STR_ENCRYPT]ExitParachute"] = 3
		}
	}
	
	--[[local FindKeys = 
	{
		["Main"] =
		{
			["s0puogdx"] = true,
			["a65ghrl1"] = true,
			["nv98vwuf"] = true,
			["en393jpx"] = true
		},
		
		["Bullet"] =
		{
			["s98o4vw0"] = true
		},
	
		["Basic"] =
		{
			["nodnfe3d"] = true
		},
		
		["ItemSystem"] =
		{
			["of6hq0km"] = true,
			["oxhesrb1"] = true
		},

		["Paraglide"] =
		{
			["roo8qard"] = true,
			["en393jpx"] = true
		}
	}]]
	
	IB_MAX_CFLOW_START()

	local Scripts = 
	{
		["[STR_ENCRYPT]Main"] = decompile(game:GetService("Players").LocalPlayer.PlayerScripts.LocalScript),
		["[STR_ENCRYPT]Bullet"] = decompile(game:GetService("ReplicatedStorage").Game.Bullets),
		["[STR_ENCRYPT]Basic"] = decompile(game:GetService("ReplicatedStorage").Game.ItemModule.Basic),
		["[STR_ENCRYPT]ItemSystem"] = decompile(game:GetService("ReplicatedStorage").Module.ItemSystem),
		["[STR_ENCRYPT]Paraglide"] = decompile(game:GetService("ReplicatedStorage").Game.Paraglide)
	}
	
	local function Deobfuscate(Key)
		local DeobfusFunc = loadstring([[
		local function FireServer(Key)
			return Key
		end
		
		return ]] .. Key)
		
		local function CreateProxy()
			local RT = newproxy(true)
			getmetatable(RT).__index = function(T, K)
				return CreateProxy()
			end
			return RT
		end
		
		local Env = {}
		setmetatable(Env, 
		{
			__index = function(T, K)
				return CreateProxy()
			end
		})
		
		setfenv(DeobfusFunc, Env)
		
		return DeobfusFunc()
	end
	
	for I,V in pairs(Scripts) do
		local Index = 1
		for ObfusKey in V:gmatch([[FireServer%b()]]) do
			local Key = Deobfuscate(ObfusKey)
			Index = Index + 1;
			for Name, TargetIndex in pairs(PreKeys[I]) do
				if rawequal(Index, TargetIndex) then
					AutoUpdate.Keys[Name] = Key
				end
			end
			--[[for Name, _ in pairs(FindKeys[I]) do
				if rawequal(Key, Name) then
					print("Found key: " .. Key .. ", Index: " .. Index .. ", in Script: " .. I)
				end
			end]]
		end
	end

	IB_MAX_CFLOW_END()
	
	if not AutoUpdate.Keys.Arrest then
		local Hint = Instance.new("Hint", workspace)
		Hint.Text = "Failed to load JailbreakHaxx. Make sure you installed the Java JDK and try again!"
		wait(5)
		Hint:Destroy()
		return
	end
	
	local RemoteName = Scripts["Main"]:match'(%w+):FireServer%(.-, %w+.Name%)';
	local FSName

	for I,V in pairs(debug.getregistry()) do
	    if typeof(V) == 'function' then
	        local Suc, Res = pcall(debug.getupvalues, V)
	        if Suc then
	            if Res[RemoteName] and typeof(Res[RemoteName]) == 'table' and Res[RemoteName].FireServer then
	                Remote = Res[RemoteName]
	                for R, _ in pairs(debug.getupvalues(Remote.FireServer)) do
	                    FSName = R
	                end
	                break
	            end
	        end
	    end
	end
	
	local RMatch = ('%w+ = true\n%s+:FS:%((.-), "NoClip StrafingNoPhysics", false%)'):gsub(':FS:', FSName)
	AutoUpdate.Keys.AntiCheat = Deobfuscate(Scripts["Main"]:match(RMatch))
	
	AutoUpdate.Keys.ClientTable = Utils.Split(Scripts["Main"]:match('UserInputType.Touch%sthen[^.]+%s1[^.]+%.[^.]+%('):match('function%s[^.]+%.[^.]+%('):match('%s[^.]+%.[^()]+'):gsub('%s', ''), '.')[1]
end

local Remote = {}
do
	local FireServer = Instance.new("RemoteEvent").FireServer
	local NameCall = getfenv()[("[STR_ENCRYPT]gbmt")]()[("[STR_ENCRYPT]__namecall")] --anti-skid protection
	if type(NameCall) ~= "function" or is_synapse_function(NameCall) or islclosure(NameCall) or #debug.getupvalues(NameCall) ~= 0 then while true do end end
	local FireFunc
	local Event
	local EventList
	local RuntimeKeys = {}
	local ReverseRuntimeKeys = {}
	local EventHooks = {}
	
	function Remote.FireServer(Key, ...)
		if Key == "[STR_ENCRYPT]Damage" then
			FireFunc(AutoUpdate.Keys[Key], ...) --todo: WTF?
		end

		local RealKey = RuntimeKeys[AutoUpdate.Keys[Key]]
		NameCall(Event(), RealKey, ..., "[STR_ENCRYPT]FireServer")
	end

	function Remote.FireServerRaw(Key, ...)
		local RealKey = RuntimeKeys[Key]
		NameCall(Event(), RealKey, ..., "[STR_ENCRYPT]FireServer")
	end
	
	function Remote.AddHook(Key, Func)
		EventHooks[AutoUpdate.Keys[Key]] = Func
	end
	
	local FireServerHook = newcclosure(function(TEvent, Key, ...)
		if EventHooks[ReverseRuntimeKeys[Key]] then EventHooks[ReverseRuntimeKeys[Key]](...) end
		if ReverseRuntimeKeys[Key] == AutoUpdate.Keys.AntiCheat then return end
		
		return FireServer(TEvent, Key, ...)
	end)
	
	--Setup hooks into Jailbreak scripts
	local BreakLoop = false
	for I,V in pairs(getreg()) do
		if BreakLoop then break end
		if type(V) == "function" and islclosure(V) then
			for I2,V2 in pairs(debug.getupvalues(V)) do
				if type(V2) == "table" and rawget(V2, "FireServer") then
					--Grab JB event handlers (retarded code)
					FireFunc = Utils.ConvertTable(debug.getupvalues(rawget(V2, "FireServer")))[1]
					local Upvals = Utils.ConvertTable(debug.getupvalues(Utils.ConvertTable(debug.getupvalues(rawget(V2, "FireServer")))[1]), true)
					local EventIdx
					local RuntimeKeysIdx
					local UpvalsIdx
					for I,V in pairs(Upvals) do
						if type(Upvals[I].Value) == "function" then
							if islclosure(Upvals[I].Value) then
								EventIdx = I
							else
								UpvalsIdx = I
							end
						elseif type(Upvals[I].Value) == "table" then
							RuntimeKeysIdx = I
						end
					end
					Event = Upvals[EventIdx].Value
					EventList = Utils.ConvertTable(debug.getupvalues(Event))[1]
					RuntimeKeys = Upvals[RuntimeKeysIdx].Value
					for I3,V3 in pairs(RuntimeKeys) do
						ReverseRuntimeKeys[V3] = I3
					end
					debug.setupvalue(Utils.ConvertTable(debug.getupvalues(rawget(V2, "FireServer")))[1], Upvals[UpvalsIdx].Key, FireServerHook)
					BreakLoop = true
					break
				end
			end
		end
	end
		
	Remote.Event = Event
	Remote.EventList = EventList
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
		return game:GetService("HttpService"):JSONDecode(readfile("jbhaxx_settings.bin"))
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
			writefile("jbhaxx_settings.bin", game:GetService("HttpService"):JSONEncode(RealSavedSettings))
		end
	end)
end

local Bypass = {}
do
	function Bypass.Teleport(...)
		local Trigger = false
		if not Settings.Get("WalkSpeedEnabled") and not Settings.Get("JumpPowerEnabled") then
			Settings.Set("WalkSpeedEnabled", true)
			Trigger = true

			wait(1)
		end

		local HRP = Utils.GetCharacter().HumanoidRootPart
		HRP.CFrame = CFrame.new(Vector3.new(...))

		if Trigger then
			wait(1)

			Settings.Set("WalkSpeedEnabled", false)
		end

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
		Remote.FireServer("Damage", Player.Character.Head.Position, Vector3.new(0, 0, 0), Player.Character.Head)
	end
	
	function Bypass.Taser(Player)
		Remote.FireServer("Taser", Player.Name, Player.Character.Head, Player.Character.Head.Position)
	end
	
	function Bypass.Arrest(Player)
		Remote.FireServer("Arrest", Player.Name)
	end
	
	function Bypass.Eject(Car)
		Remote.FireServer("Eject", Car)
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
	
	--Disable ragdoll + nitro checks
	local BreakLoop = false
	local RagdollTable
	local ClientTable
	local SetNitro
	local KnowArrest
	for I,V in pairs(getreg()) do
		if BreakLoop then break end
		if type(V) == "function" and islclosure(V) then
			for I2,V2 in pairs(debug.getupvalues(V)) do
				if type(V2) == "table" and rawget(V2, "LastVehicleExit") then
					RagdollTable = V2
					BreakLoop = true
					break
				end
			end
		end
	end
		
	BreakLoop = false

	for I,V in pairs(getreg()) do
		if BreakLoop then break end
		if type(V) == "function" and islclosure(V) and not is_synapse_function(V) then
			for I2,V2 in pairs(debug.getupvalues(V)) do
				if I2 == AutoUpdate.Keys.ClientTable then
					ClientTable = V2
					BreakLoop = true
					break
				end
			end
		end
	end
	
	for I,V in pairs(ClientTable) do
		if type(V) == "function" and not is_synapse_function(V) then
			for I2,V2 in pairs(debug.getconstants(V)) do
				if V2 == "%d/%d Fuel" then
					SetNitro = V
				end
				if V2 == "%s Cash" then
					KnowArrest = I
				end
			end
		end
	end

	RagdollTable.LastVehicleExit = nil
		
	setmetatable(RagdollTable, 
	{
		__index = function(T, K)
			if K == "LastVehicleExit" then
				return tick() + 10
			end
			
			return rawget(RagdollTable, K)
		end,
		
		__newindex = function(T, K, V)
			if K == "LastVehicleExit" then
				rawset(RagdollTable, "LastVehicleExit", nil)
				return
			end
			
			return rawset(RagdollTable, K, V)
		end
	})
	
	local CurrentlyArresting = false
	local CurrentlyKilling = false
	local ArrestComplete = false
	local FirstArrest = true
	local ArrestBlacklist = {}
	local TpAuraBlacklist = {}
	
	for I,V in pairs(Remote.EventList) do
		V.OnClientEvent:Connect(function(Key, ...)
			local Args = {...}
			
			if CurrentlyArresting and Key == KnowArrest and Args[2] == "Arrest" then
				ArrestComplete = true
			end
		end)
	end
	
	game:GetService("RunService").Stepped:Connect(function()
		if Settings.Get("InfNitro") then
			SetNitro(300, 300)
		end

		local Char = Utils.GetCharacter()
		if Settings.Get("WalkSpeedEnabled") or Settings.Get("JumpPowerEnabled") then	
			Remote.FireServer("Parachute")
			if Char:FindFirstChild("Parachute") then
				Char.Parachute:Destroy()
			end
		else
			Remote.FireServer("ExitParachute")
		end
		
		if Settings.Get("NoClip") then
			Char.Humanoid:ChangeState(11)
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
					if not FirstArrest then
						if Utils.GetDistance(Char.HumanoidRootPart, Near.Character.HumanoidRootPart) > 150 then
							game:GetService("StarterGui"):SetCore("SendNotification",
							{
								Title = "JBHaxx AutoArrest",
								Text = "AutoArrest is waiting for the next arrest slot to be open. Please be patient.",
								Duration = 30
							})

							wait(30)
						else
							game:GetService("StarterGui"):SetCore("SendNotification",
							{
								Title = "JBHaxx AutoArrest",
								Text = "AutoArrest is waiting for the next arrest slot to be open. Please be patient.",
								Duration = 5
							})

							wait(5)
						end
					else
						FirstArrest = false
					end
					if not ArrestBlacklist[Near] then
						CurrentlyArresting = false
						ArrestComplete = false
					end
					local Pos = Near.Character.HumanoidRootPart.CFrame.p
					Bypass.Teleport(Pos.x, Pos.y, Pos.z)
					wait(3)
					while tostring(Near.Team) == "Criminal" and not ArrestComplete and Settings.Get("AutoArrest") do
						local Resolve = Utils.ResolveVehicle(Near)
						if Resolve then
							Bypass.Eject(Resolve)
						end
						Bypass.Taser(Near)
						Bypass.Arrest(Near)
						Utils.GetCharacter().HumanoidRootPart.CFrame = Near.Character.HumanoidRootPart.CFrame - Vector3.new(3, 0, 0)
						wait(3)
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
	
	Remote.AddHook("Bullet", function()
		if Settings.Get("TriggerBot") then
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
	local Render = loadstring(game:HttpGet("[STR_ENCRYPT]https://cdn.synapse.to/synapsedistro/hub/HaxxMenu.lua", true))()
	loadstring(game:HttpGet("[STR_ENCRYPT]https://cdn.synapse.to/synapsedistro/hub/ESPLib.lua", true))()
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
	local TriggerBot = CombatMenu.Add("toggle", "Trigger Bot")
	local AutoArrest = CombatMenu.Add("toggle", "Auto Arrest")
	local InfAmmo = CombatMenu.Add("clickable", "Infinite Ammo")
	local MachinePistol = CombatMenu.Add("clickable", "Machine Pistol")
	
	local Speed = MovementMenu.Add("toggle", "Speed")
	local SuperJump = MovementMenu.Add("toggle", "Super Jump")
	local NoClip = MovementMenu.Add("toggle", "NoClip")
	local ClickTeleport = MovementMenu.Add("toggle", "Click Teleport")
	
	local ESP = RenderMenu.Add("toggle", "ESP")
	local Tracers = RenderMenu.Add("toggle", "Tracers")
	
	local CrimBase = TeleportsMenu.Add("clickable", "Criminal Base")
	local DonutShop = TeleportsMenu.Add("clickable", "Donut Shop")
	local Jail = TeleportsMenu.Add("clickable", "Jail")
	local Jewelry = TeleportsMenu.Add("clickable", "Jewelry")
	local Bank = TeleportsMenu.Add("clickable", "Bank")
	local GasStation = TeleportsMenu.Add("clickable", "Gas Station")
	local Museum = TeleportsMenu.Add("clickable", "Museum")
	
	local InfNitro = UtilityMenu.Add("toggle", "Infinite Nitro")
	
	local SpeedPlus = SettingsMenu.Add("clickable", "Speed +")
	local SpeedMinus = SettingsMenu.Add("clickable", "Speed -")
	local SuperJumpPlus = SettingsMenu.Add("clickable", "Super Jump +")
	local SuperJumpMinus = SettingsMenu.Add("clickable", "Super Jump -")
	local DisableBlur = SettingsMenu.Add("toggle", "Disable Blur")
	
	CrimBase.Callback = function()
		Bypass.Teleport(-226, 18, 1590)
	end
	
	DonutShop.Callback = function()
		Bypass.Teleport(268, 18, -1760)
	end
	
	Jail.Callback = function()
		Bypass.Teleport(-1133, 18, -1355)
	end
	
	Jewelry.Callback = function()
		Bypass.Teleport(142, 18, 1365)
	end
	
	Bank.Callback = function()
		Bypass.Teleport(10, 18, 784)
	end
	
	GasStation.Callback = function()
		Bypass.Teleport(-1583, 18, 724)
	end
	
	Museum.Callback = function()
		Bypass.Teleport(1158, 102, 1272)
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
	
	TriggerBot.Callback = function(Type, Name, Value)
		Settings.Set("TriggerBot", Value)
	end
	
	ESP.Callback = function(Type, Name, Value)
		ESPLib.Enabled = Value
	end
	
	Tracers.Callback = function(Type, Name, Value)
		ESPLib.Settings.DrawTracers = Value
	end
	
	InfAmmo.Callback = function()
		for I,V in pairs(getreg()) do
			if type(V) == "function" and islclosure(V) then
				for I2,V2 in pairs(debug.getupvalues(V)) do
					if type(V2) == "table" and rawget(V2, "AmmoCurrent") then
						V2.AmmoCurrent = math.huge
					end
					
					if type(V2) == "table" and rawget(V2, "Pistol") then
						for I3,V3 in pairs(V2) do
							if rawget(V3, "FireFreq") then
								V3.MagSize = math.huge
							end
						end
					end
				end
			end
		end
	end
		
	MachinePistol.Callback = function()
		for I,V in pairs(getreg()) do
			if type(V) == "function" and islclosure(V) then
				for I2,V2 in pairs(debug.getupvalues(V)) do
					if type(V2) == "table" and rawget(V2, "Pistol") then
						for I3,V3 in pairs(V2) do
							if rawget(V3, "FireFreq") then
								V3.FireFreq = math.huge
								V3.CamShakeMagnitude = 1
								V3.FireAuto = true
							end
						end
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