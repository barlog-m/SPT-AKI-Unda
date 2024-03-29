import { ApplicationContext } from "@spt-aki/context/ApplicationContext";
import { DurabilityLimitsHelper } from "@spt-aki/helpers/DurabilityLimitsHelper";
import { ItemHelper } from "@spt-aki/helpers/ItemHelper";
import { Item, Repairable, Upd } from "@spt-aki/models/eft/common/tables/IItem";
import { ITemplateItem } from "@spt-aki/models/eft/common/tables/ITemplateItem";
import { IChooseRandomCompatibleModResult } from "@spt-aki/models/spt/bots/IChooseRandomCompatibleModResult";
import { EquipmentFilters, IBotConfig, IRandomisedResourceValues } from "@spt-aki/models/spt/config/IBotConfig";
import { IPmcConfig } from "@spt-aki/models/spt/config/IPmcConfig";
import { ILogger } from "@spt-aki/models/spt/utils/ILogger";
import { ConfigServer } from "@spt-aki/servers/ConfigServer";
import { DatabaseServer } from "@spt-aki/servers/DatabaseServer";
import { LocalisationService } from "@spt-aki/services/LocalisationService";
import { JsonUtil } from "@spt-aki/utils/JsonUtil";
import { RandomUtil } from "@spt-aki/utils/RandomUtil";
export declare class BotGeneratorHelper {
    protected logger: ILogger;
    protected randomUtil: RandomUtil;
    protected databaseServer: DatabaseServer;
    protected durabilityLimitsHelper: DurabilityLimitsHelper;
    protected itemHelper: ItemHelper;
    protected applicationContext: ApplicationContext;
    protected localisationService: LocalisationService;
    protected configServer: ConfigServer;
    protected botConfig: IBotConfig;
    protected pmcConfig: IPmcConfig;
    constructor(logger: ILogger, randomUtil: RandomUtil, databaseServer: DatabaseServer, durabilityLimitsHelper: DurabilityLimitsHelper, itemHelper: ItemHelper, applicationContext: ApplicationContext, localisationService: LocalisationService, configServer: ConfigServer);
    /**
     * Adds properties to an item
     * e.g. Repairable / HasHinge / Foldable / MaxDurability
     * @param itemTemplate Item extra properties are being generated for
     * @param botRole Used by weapons to randomize the durability values. Null for non-equipped items
     * @returns Item Upd object with extra properties
     */
    generateExtraPropertiesForItem(itemTemplate: ITemplateItem, botRole?: string): {
        upd?: Upd;
    };
    /**
     * Randomize the HpResource for bots e.g (245/400 resources)
     * @param maxResource Max resource value of medical items
     * @param randomizationValues Value provided from config
     * @returns Randomized value from maxHpResource
     */
    protected getRandomizedResourceValue(maxResource: number, randomizationValues: IRandomisedResourceValues): number;
    /**
     * Get the chance for the weapon attachment or helmet equipment to be set as activated
     * @param botRole role of bot with weapon/helmet
     * @param setting the setting of the weapon attachment/helmet equipment to be activated
     * @param defaultValue default value for the chance of activation if the botrole or bot equipment role is null
     * @returns Percent chance to be active
     */
    protected getBotEquipmentSettingFromConfig(botRole: string, setting: keyof EquipmentFilters, defaultValue: number): number;
    /**
     * Create a repairable object for a weapon that containers durability + max durability properties
     * @param itemTemplate weapon object being generated for
     * @param botRole type of bot being generated for
     * @returns Repairable object
     */
    protected generateWeaponRepairableProperties(itemTemplate: ITemplateItem, botRole: string): Repairable;
    /**
     * Create a repairable object for an armor that containers durability + max durability properties
     * @param itemTemplate weapon object being generated for
     * @param botRole type of bot being generated for
     * @returns Repairable object
     */
    protected generateArmorRepairableProperties(itemTemplate: ITemplateItem, botRole: string): Repairable;
    isWeaponModIncompatibleWithCurrentMods(itemsEquipped: Item[], tplToCheck: string, modSlot: string): IChooseRandomCompatibleModResult;
    /**
     * Can item be added to another item without conflict
     * @param itemsEquipped Items to check compatibilities with
     * @param tplToCheck Tpl of the item to check for incompatibilities
     * @param equipmentSlot Slot the item will be placed into
     * @returns false if no incompatibilities, also has incompatibility reason
     */
    isItemIncompatibleWithCurrentItems(itemsEquipped: Item[], tplToCheck: string, equipmentSlot: string): IChooseRandomCompatibleModResult;
    /**
     * Convert a bots role to the equipment role used in config/bot.json
     * @param botRole Role to convert
     * @returns Equipment role (e.g. pmc / assault / bossTagilla)
     */
    getBotEquipmentRole(botRole: string): string;
}
/** TODO - move into own class */
export declare class ExhaustableArray<T> {
    private itemPool;
    private randomUtil;
    private jsonUtil;
    private pool;
    constructor(itemPool: T[], randomUtil: RandomUtil, jsonUtil: JsonUtil);
    getRandomValue(): T;
    getFirstValue(): T;
    hasValues(): boolean;
}
