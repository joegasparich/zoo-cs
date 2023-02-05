using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raylib_cs;
using Zoo.defs;
using Zoo.ui;
using Zoo.util;

namespace Zoo.entities;

public enum PersonAgeCategory {
    Child,
    Adult,
    Elder
}

public enum PersonGender {
    Male,
    Female,
    NonBinary
}

public class PersonComponentData : ComponentData {
    public override Type CompClass => typeof(PersonComponent);

    // Overrides Random
    public PersonGender?      Gender;
    public PersonAgeCategory? Age;
    public string?            Body;
    public string?            Hair;
    public string?            Beard;
    public string?            Shirt;
    public string?            Pants;
    public string?            Hat;
}

public class PersonComponent : Component {
    public static Type DataType => typeof(PersonComponentData);

    public static List<string> AdultBodies     = new();
    public static List<string> ElderBodies     = new();
    public static List<string> ChildBodies     = new();
    public static List<string> AllHats         = new();
    public static List<string> MaleHair        = new();
    public static List<string> FemaleHair      = new();
    public static List<string> MaleElderHair   = new();
    public static List<string> FemaleElderHair = new();
    public static List<string> Beards          = new();
    public static List<string> ElderBeards     = new();
    public static List<string> AdultShirts     = new();
    public static List<string> AdultPants      = new();
    public static List<string> ChildShirts     = new();
    public static List<string> ChildPants      = new();

    public static List<string> MaleNames = new();
    public static List<string> FemaleNames = new();
    public static List<string> LastNames  = new();
    
    // Constants
    // This is to prevent skin and hair colour being identical because of the colour palette, please don't cancel me!
    public static Dictionary<(string, string), string> HairSkinColourBlackList = new() {
        { ("4", "brown"), "black" }
    };
    
    // Config
    public PersonAgeCategory AgeCategory;
    public PersonGender      Gender;

    // State
    private string firstName;
    private string lastName;

    private string? body;
    private string? hair;
    private string? beard;
    private string? hat;
    private string? shirt;
    private string? pants;
    
    // Properties
    public             PersonComponentData Data         => (PersonComponentData)data;
    protected override Type[]              Dependencies => new[] { typeof(RenderComponent) };
    private            RenderComponent     Renderer     => entity.GetComponent<RenderComponent>();
    public             string              FullName     => $"{firstName.Capitalise()} {lastName.Capitalise()}";

    public PersonComponent(Entity entity, PersonComponentData? data) : base(entity, data) {}

    public override void Start() {
        base.Start();
        
        // Age
        AgeCategory = Data.Age ?? Rand.EnumValue<PersonAgeCategory>();

        // Gender
        if (Data.Gender != null)
            Gender = Data.Gender.Value;
        else if (Rand.Chance(0.05f))
            Gender = PersonGender.NonBinary;
        else
            Gender = Rand.Bool() ? PersonGender.Male : PersonGender.Female;

        // Generate name
        firstName = Gender switch {
            PersonGender.Male      => MaleNames.RandomElement(),
            PersonGender.Female    => FemaleNames.RandomElement(),
            PersonGender.NonBinary => Rand.Bool() ? MaleNames.RandomElement() : FemaleNames.RandomElement(),
        };
        lastName = LastNames.RandomElement();
        
        // Pick outfit
        if (body == null)
            PickOutfit(AgeCategory, Gender);

        Renderer.BaseGraphic.SetSprite(body);
        if (pants != null) Renderer.AddAttachment(pants);
        if (shirt != null) Renderer.AddAttachment(shirt);
        if (beard != null) Renderer.AddAttachment(beard);
        if (hair  != null) Renderer.AddAttachment(hair);
    }

    private void PickOutfit(PersonAgeCategory age, PersonGender gender) {
        switch (age) {
            case PersonAgeCategory.Adult:
                body = AdultBodies.RandomElement();
                shirt = AdultShirts.RandomElement();
                pants = AdultPants.RandomElement();
                if (gender == PersonGender.Male || gender == PersonGender.NonBinary)
                    if (Rand.Chance(0.3f)) beard = Beards.RandomElement();
                if (gender == PersonGender.Male)
                    if (Rand.Chance(0.9f)) hair = MaleHair.RandomElement();
                if (gender == PersonGender.Female)
                    hair = FemaleHair.RandomElement();
                if (gender == PersonGender.NonBinary)
                    if (Rand.Chance(0.9f)) hair = Rand.Bool() ? MaleHair.RandomElement() : FemaleHair.RandomElement();
                break;
            case PersonAgeCategory.Elder:
                body = ElderBodies.RandomElement();
                shirt = AdultShirts.RandomElement();
                pants = AdultPants.RandomElement();
                if (gender == PersonGender.Male || gender == PersonGender.NonBinary)
                    if (Rand.Chance(0.3f)) beard = ElderBeards.RandomElement();
                if (gender == PersonGender.Male)
                    hair = MaleElderHair.RandomElement();
                if (gender == PersonGender.Female)
                    hair = FemaleElderHair.RandomElement();
                if (gender == PersonGender.NonBinary)
                    if (Rand.Chance(0.9f)) hair = Rand.Bool() ? MaleElderHair.RandomElement() : FemaleElderHair.RandomElement();
                break;
            case PersonAgeCategory.Child:
                body = ChildBodies.RandomElement();
                shirt = ChildShirts.RandomElement();
                pants = ChildPants.RandomElement();
                if (gender == PersonGender.Male)
                    hair = MaleHair.RandomElement();
                if (gender == PersonGender.Female)
                    hair = FemaleHair.RandomElement();
                if (gender == PersonGender.NonBinary)
                    if (Rand.Chance(0.9f)) hair = Rand.Bool() ? MaleHair.RandomElement() : FemaleHair.RandomElement();
                break;
        }
        
        // Prevent hair and skin colour being identical
        if (hair != null) {
            var skinTone   = body[body.IndexOf(".") - 1].ToString();
            var hairColour = hair.Split("_")[1];
            if (HairSkinColourBlackList.ContainsKey((skinTone, hairColour))) {
                hair = hair.Replace(hairColour, HairSkinColourBlackList[(skinTone, hairColour)]);
            }
            
            // Match hair and beard colour
            if (beard != null) {
                var beardColour = beard.Split("_")[1];
                beard = beard.Replace(beardColour, hairColour);
            }
        }

        // Override if specified in data
        if (Data.Body  != null) body  = Data.Body;
        if (Data.Hair  != null) hair  = Data.Hair;
        if (Data.Beard != null) beard = Data.Beard;
        if (Data.Shirt != null) shirt = Data.Shirt;
        if (Data.Pants != null) pants = Data.Pants;
        if (Data.Hat   != null) hat   = Data.Hat;
    }

    public override InfoTab? GetInfoTab() {
        return new InfoTab("Needs", rect => {
            var listing = new Listing(rect);
            listing.Label(FullName);
        });
    }

    public override void Serialise() {
        base.Serialise();
        
        Find.SaveManager.ArchiveValue("body", ref body);
        Find.SaveManager.ArchiveValue("hair", ref hair);
        Find.SaveManager.ArchiveValue("beard", ref beard);
        Find.SaveManager.ArchiveValue("hat", ref hat);
        Find.SaveManager.ArchiveValue("shirt", ref shirt);
        Find.SaveManager.ArchiveValue("pants", ref pants);
    }

    public static Texture2D GetPersonTexture(ActorDef def) {
        var renderData = def.Components.Find(comp => comp is RenderComponentData) as RenderComponentData;
        var personData = def.Components.Find(comp => comp is PersonComponentData) as PersonComponentData;

        var iconPerson = new PersonComponent(null, personData);
        iconPerson.PickOutfit(personData.Age ?? PersonAgeCategory.Adult, personData.Gender ?? Rand.EnumValue<PersonGender>());

        var renderer = new RenderComponent(null, renderData);
        renderer.BaseGraphic.SetSprite(iconPerson.body);
        if (iconPerson.pants != null) renderer.AddAttachment(iconPerson.pants);
        if (iconPerson.shirt != null) renderer.AddAttachment(iconPerson.shirt);
        if (iconPerson.beard != null) renderer.AddAttachment(iconPerson.beard);
        if (iconPerson.hair  != null) renderer.AddAttachment(iconPerson.hair);

        return renderer.Graphics.Texture;
    }
}
