---
--- Created by Z_End.
--- DateTime: 2025/3/7 23:19
---

---@alias OnCardActionDelegate fun(rec:InGameCardBridge,give:(InGameCardBridge|nil)
---@alias BatchEnumeratorsActionDelegate fun(cache:List<IEnumerator>)

---@generic T
---@class List<T>
---@field Add fun(self:List<T>,val:T)

---@class IEnumerator

---@class Events
---@field RegModLoadComplete fun(self:Events,action:fun():void)
---@field RegModLoadCompletePost fun(self:Events,action:fun():void)
---@field RegOnAction fun(self:Events,key:string,action:OnCardActionDelegate))
---@field RegOnEndAction fun(self:Events,key:string,action:OnCardActionDelegate))
---@field RegCardNameOverride fun(self:Events,uobj:UniqueIdObjectBridge,func:fun(card:InGameCardBridge):string))
---@field RegCardDescOverride fun(self:Events,uobj:UniqueIdObjectBridge,func:fun(card:InGameCardBridge):string))
---@field RegActionNameOverride fun(self:Events,uobj:UniqueIdObjectBridge,func:fun(actionName:string,card:InGameCardBridge):string))
Events = {}

---@class CoroutineHelper
---@field BatchEnumerators fun(self:CoroutineHelper,action:BatchEnumeratorsActionDelegate)
CoroutineHelper = {}

---@alias FadeToBlackTypes ("None"|"Partial"|"Full")
---@alias CardActionType ("CardAction"|"CardOnCardAction"|"DismantleCardAction"|"FromStatChangeAction")
---@alias CardTypes ("Item"|"Base"|"Location"|"Event"|"Environment"|"Weather"|"Hand"|"Blueprint"|"Explorable"|"Liquid"|"EnvImprovement"|"EnvDamage"|"BlueprintInLocation"|"InvisibleCard")
---@class GameBridge
---@field CreateAction fun(self:GameBridge,id:string,name:string,type:CardActionType):CardActionBridge
---@field CreateCard fun(self:GameBridge,id:string,name:string,type:CardTypes,icon:string):UniqueIdObjectBridge
---@field Log fun(self:GameBridge,message:string)
---@field PassTime fun(self:GameBridge,miniTick:number,fromCard:InGameCardBridge,blockable:boolean,fadeType:FadeToBlackTypes,fadeText:string)
---@field PassTimeEnum fun(self:GameBridge,miniTick:number,fromCard:InGameCardBridge,blockable:boolean,fadeType:FadeToBlackTypes,fadeText:string):IEnumerator
---@field GetItem fun(self:GameBridge,key:string):(UniqueIdObjectBridge|nil)
---@field CloseCurrentInspectionPopup fun(self:GameBridge)
---@field RefreshCurrentInspectionPopup fun(self:GameBridge)
---@field FindCards fun(self:GameBridge,cardData:UniqueIdObjectBridge,cache:InGameCardBridge[],includeBackground:boolean|nil)
---@field MiniTicksPerTick number
Game = {}

---@class AddCardExtraArg
---@field Trans boolean
---
