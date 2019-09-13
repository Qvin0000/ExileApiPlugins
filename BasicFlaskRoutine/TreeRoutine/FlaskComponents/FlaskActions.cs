namespace TreeRoutine.FlaskComponents
{
    public enum FlaskActions
    {
        Ignore = 0,         // ignore mods and don't give error
        None,               // flask isn't initilized.
        Life,               //life, Blood of the Karui
        Mana,               //mana, Doedre's Elixir, 
                            //Zerphi's Last Breath, Lavianga's Spirit

        Hybrid,             //hybrid flasks,

        Defense,            //bismuth, jade, stibnite, granite,
                            //amethyst, ruby, sapphire, topaz,
                            // aquamarine, quartz, Sin's Rebirth, 
                            //Coruscating Elixir, Forbidden Taste,Rumi's Concoction
                            //MODS: iron skin, reflexes, gluttony,
                            // craving, resistance

        Utility,            //Doedre's Elixir, Zerphi's Last Breath, Lavianga's Spirit

        Speedrun,           //quick silver, MOD: adrenaline,

        Offense,            //silver, sulphur, basalt, diamond,Taste of Hate, 
                            //Kiara's Determination, Lion's Roar, The Overflowing Chalice, 
                            //The Sorrow of the Divine,Rotgut, Witchfire Brew, Atziri's Promise, 
                            //Dying Sun,Vessel of Vinktar
                            //MOD: Fending

        PoisonImmune,      // MOD: curing
        FreezeImmune,      // MOD: heat
        IgniteImmune,      // MOD: dousing
        ShockImmune,       // MOD: grounding
        BleedImmune,       // MOD: staunching
        CurseImmune,       // MOD: warding
        UniqueFlask,       // All the milk shakes
        OFFENSE_AND_SPEEDRUN,//Silver Flask, for SpeedFlaskLogic and OffensiveFlask
    }
}
