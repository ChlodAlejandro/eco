using System.Collections.Generic;
using UnityEngine;

public class GeneUtility
{
    private static bool RandomBool { get { return Random.Range(0, 2) == 1; } }

    public static bool Relatives(Creature a, Creature b)
    {
        if (a.Generation == 1 && b.Generation == 1) return false;

        List<Creature> toCheck = new List<Creature>();

        // LOTS OF NULLS!

        toCheck.Add(a);
        toCheck.Add(b);
        if (a.father != null) toCheck.Add(a.father);
        if (b.father != null) toCheck.Add(b.father);
        if (a.mother != null) toCheck.Add(a.mother);
        if (b.mother != null) toCheck.Add(b.mother);
        if (a.father != null && a.father.father != null) toCheck.Add(a.father.father);
        if (a.father != null && a.father.mother != null) toCheck.Add(a.father.mother);
        if (a.mother != null && a.mother.father != null) toCheck.Add(a.mother.father);
        if (a.mother != null && a.mother.mother != null) toCheck.Add(a.mother.mother);
        if (b.father != null && b.father.father != null) toCheck.Add(b.father.father);
        if (b.father != null && b.father.mother != null) toCheck.Add(b.father.mother);
        if (b.mother != null && b.mother.father != null) toCheck.Add(b.mother.father);
        if (b.mother != null && b.mother.mother != null) toCheck.Add(b.mother.mother);

        Creature[] family = toCheck.ToArray();

        for (int ca = 0; ca < family.Length; ca++)
        {
            for (int cb = 0; cb < family.Length; cb++)
            {
                if (ca == cb) continue;
                if (family[ca] != null && family[cb] != null && family[ca].ID == family[cb].ID)
                    return true;
            }
        }
        return false;
    }

    public static Creature GetLink(Creature a, Creature b)
    {
        if (a.Generation == 1 && b.Generation == 1) return null;

        List<Creature> toCheck = new List<Creature>();

        // LOTS OF NULLS!

        toCheck.Add(a);
        toCheck.Add(b);
        if (a.father != null) toCheck.Add(a.father);
        if (b.father != null) toCheck.Add(b.father);
        if (a.mother != null) toCheck.Add(a.mother);
        if (b.mother != null) toCheck.Add(b.mother);
        if (a.father != null && a.father.father != null) toCheck.Add(a.father.father);
        if (a.father != null && a.father.mother != null) toCheck.Add(a.father.mother);
        if (a.mother != null && a.mother.father != null) toCheck.Add(a.mother.father);
        if (a.mother != null && a.mother.mother != null) toCheck.Add(a.mother.mother);
        if (b.father != null && b.father.father != null) toCheck.Add(b.father.father);
        if (b.father != null && b.father.mother != null) toCheck.Add(b.father.mother);
        if (b.mother != null && b.mother.father != null) toCheck.Add(b.mother.father);
        if (b.mother != null && b.mother.mother != null) toCheck.Add(b.mother.mother);

        Creature[] family = toCheck.ToArray();

        for (int ca = 0; ca < family.Length; ca++)
        {
            for (int cb = 0; cb < family.Length; cb++)
            {
                if (ca == cb) continue;
                if (family[ca] != null && family[cb] != null && family[ca].ID == family[cb].ID)
                    return family[ca];
            }
        }
        return null;
    }
}
