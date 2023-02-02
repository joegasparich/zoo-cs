﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raylib_cs;
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
    // Static cache TODO: Move this somewhere
    private static bool         loadedIntoCache = false;

    private static List<string> AdultBodies     = new();
    private static List<string> ElderBodies     = new();
    private static List<string> ChildBodies     = new();
    private static List<string> AllHats         = new();
    private static List<string> MaleHair        = new();
    private static List<string> FemaleHair      = new();
    private static List<string> MaleElderHair   = new();
    private static List<string> FemaleElderHair = new();
    private static List<string> Beards          = new();
    private static List<string> ElderBeards     = new();
    private static List<string> AdultShirts     = new();
    private static List<string> AdultPants      = new();
    private static List<string> ChildShirts     = new();
    private static List<string> ChildPants      = new();

    private static List<string> MaleNames = new();
    private static List<string> FemaleNames = new();
    private static List<string> LastNames  = new();
    
    // Constants
    // This is to prevent skin and hair colour being identical because of the colour palette, please don't cancel me!
    public static Dictionary<(string, string), string> HairSkinColourBlackList = new() {
        { ("4", "brown"), "black" }
    };
    
    // Config
    public PersonAgeCategory AgeCategory;
    public PersonGender      Gender;

    // References
    private RenderComponent renderer;
    
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
    public PersonComponentData Data => (PersonComponentData)data;
    protected override Type[] Dependencies => new[] { typeof(RenderComponent) };
    public string FullName => $"{firstName.Capitalise()} {lastName.Capitalise()}";

    public PersonComponent(Entity entity, PersonComponentData? data) : base(entity, data) {}

    public override void Start() {
        base.Start();
        
        renderer = entity.GetComponent<RenderComponent>();
        
        // TODO: Move this somewhere so we aren't hitting the file system after launch
        if (!loadedIntoCache) {
            LoadTextures(AdultBodies,     "assets/textures/people/body");
            LoadTextures(ElderBodies,     "assets/textures/people/body/old");
            LoadTextures(ChildBodies,     "assets/textures/people/body/child");
            LoadTextures(AllHats,         "assets/textures/people/hats");
            LoadTextures(MaleHair,        "assets/textures/people/hair/male");
            LoadTextures(FemaleHair,      "assets/textures/people/hair/female");
            LoadTextures(MaleElderHair,   "assets/textures/people/hair/old/male");
            LoadTextures(FemaleElderHair, "assets/textures/people/hair/old/female");
            LoadTextures(Beards,          "assets/textures/people/beards");
            LoadTextures(ElderBeards,     "assets/textures/people/beards/old");
            LoadTextures(AdultShirts,     "assets/textures/people/shirts");
            LoadTextures(ChildShirts,     "assets/textures/people/shirts/child");
            LoadTextures(AdultPants,      "assets/textures/people/pants");
            LoadTextures(ChildPants,      "assets/textures/people/pants/child");

            var json = File.ReadAllText("assets/lang/names.json");
            var data = JsonConvert.DeserializeObject<JObject>(json)!;
            MaleNames   = data["maleFirstNames"].ToObject<List<String>>();
            FemaleNames = data["femaleFirstNames"].ToObject<List<String>>();
            LastNames   = data["lastNames"].ToObject<List<String>>();

            loadedIntoCache = true;
        }
        
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

        renderer.BaseGraphic.SetSprite(body);
        if (pants != null) renderer.AddAttachment(pants);
        if (shirt != null) renderer.AddAttachment(shirt);
        if (beard != null) renderer.AddAttachment(beard);
        if (hair  != null) renderer.AddAttachment(hair);
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
                    if (Rand.Chance(0.9f)) MaleHair.RandomElement();
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

    private void LoadTextures(List<string> list, string path) {
        try {
            foreach (var file in FileUtility.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly)) {
                list.Add(file);
            }
        } catch (Exception e) {
            Debug.Error("Failed to load textures from " + path + ": " + e.Message);
        }
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
}
