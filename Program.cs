using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;

class Player
{
    public const int TYPE_MONSTER = 0;
    public const int TYPE_MY_HERO = 1;
    public const int TYPE_OP_HERO = 2;

    public class Entity
    {
        public int Id;
        public int Type;
        public int X, Y;
        public int ShieldLife;
        public int IsControlled;
        public int Health;
        public int Vx, Vy;
        public int NearBase;
        public int ThreatFor;

        public Entity(int id, int type, int x, int y, int shieldLife, int isControlled, int health, int vx, int vy, int nearBase, int threatFor)
        {
            this.Id = id;
            this.Type = type;
            this.X = x;
            this.Y = y;
            this.ShieldLife = shieldLife;
            this.IsControlled = isControlled;
            this.Health = health;
            this.Vx = vx;
            this.Vy = vy;
            this.NearBase = nearBase;
            this.ThreatFor = threatFor;
        }
    }

    static void Main(string[] args)
    {

        //Compare la "distance" entre le Héros et le monstre, retourne la distance entre eux deux.
        static int compareTwoDistance(int positionMob, int positionHero)
        {
            positionMob = Math.Abs(positionMob);
            positionHero = Math.Abs(positionHero);
            return positionMob - positionHero;
        }

        //Permet de savoir quelle est le monstre le plus proche du "coeur" de la base, et le prendre pour cible
        static List<object> compareDistanceBetweenMonsterAndBase(Entity monster, int previousPositionBetweenMonsterAndBase, int positionBetweenMonsterAndBase, int sideOfPlayer)
        {
            Entity target = null;

            //Si aucun ennemi pris pour cible, initialise
            if (previousPositionBetweenMonsterAndBase == 0)
            {
                previousPositionBetweenMonsterAndBase = monster.X + monster.Y;
                positionBetweenMonsterAndBase = monster.X + monster.Y;
                target = monster;
            }

            //Si déja un ennemi en cible, alors prend une seconde variable pour comparer la précédente cible a la nouvelle
            else
            {
                positionBetweenMonsterAndBase = monster.X + monster.Y;

                //Si le 2ème ennemi est plus proche du coeur de la base, alors le prend pour cible
                if ((sideOfPlayer == 0 && positionBetweenMonsterAndBase < previousPositionBetweenMonsterAndBase) || (sideOfPlayer == 1 && positionBetweenMonsterAndBase > previousPositionBetweenMonsterAndBase))
                {
                    previousPositionBetweenMonsterAndBase = positionBetweenMonsterAndBase;
                    target = monster;
                }
            }
            List<object> listResult = new List<object>();
            listResult.Add(target);
            listResult.Add(previousPositionBetweenMonsterAndBase);
            return listResult;
        }

        //Permet de savoir quelle est le monstre le plus proche du héros, le prend pour cible
        static List<object> compareDistanceBetweenMonsterAndHeros(Entity monster, Entity heroes, int previousPositionBetweenMonsterAndHero, int positionBetweenMonsterAndHero, int sideOfPlayer)
        {
            Entity target = null;

            int positionHero = heroes.X + heroes.Y;
            int positionMonster = monster.X + monster.Y;

            //Si aucun ennemi pris pour cible, initialise
            if (previousPositionBetweenMonsterAndHero == 0)
            {
                previousPositionBetweenMonsterAndHero = compareTwoDistance(positionMonster, positionHero);
                positionBetweenMonsterAndHero = previousPositionBetweenMonsterAndHero;
                target = monster;
            }

            //Si déja un ennemi en cible, alors prend une seconde variable pour comparer la précédente cible a la nouvelle
            else
            {
                positionBetweenMonsterAndHero = compareTwoDistance(positionMonster, positionHero);

                //Si le 2ème ennemi est plus proche du héros de le 1er, alors le prend pour cible
                if ((sideOfPlayer == 0 && positionBetweenMonsterAndHero < previousPositionBetweenMonsterAndHero) || (sideOfPlayer == 1 && positionBetweenMonsterAndHero > previousPositionBetweenMonsterAndHero))
                {
                    previousPositionBetweenMonsterAndHero = positionBetweenMonsterAndHero;
                    target = monster;
                }
            }
            List<object> listResult = new List<object>();
            listResult.Add(target);
            listResult.Add(previousPositionBetweenMonsterAndHero);
            return listResult;
        }

        string[] inputs;
        inputs = Console.ReadLine().Split(' ');

        // base_x,base_y: The corner of the map representing your base
        int baseX = int.Parse(inputs[0]);
        int baseY = int.Parse(inputs[1]);

        // heroesPerPlayer: Always 3
        int heroesPerPlayer = int.Parse(Console.ReadLine());

        //Compte le nombre de tour écoulé.
        int turnCount = -1;

        //Garde la position "de base" pour chaque héros (Défini au tour 7).
        List<int> defaultPositionX = new List<int>();
        List<int> defaultPositionY = new List<int>();

        //Permet de pouvoir faire des actions spécifique en fonction du numéro.
        //stylePlay = 0: Jeu normal, bats les ennemis et cast des CONTROL pour en envoyer dans le camp ennemi.
        //stylePlay = 1: Jeu defensif, les héros vont dans la base éléminer les monstres si ils sont trop proche.
        int stylePlay = 0;

        //Permet de pouvoir savoir si on est du coté Bleu ou Rouge.
        int sideOfPlayer = 0;
        sideOfPlayer = (baseX == 0) ? 0 : 1;

        //Permet de dire que, leur position a ce tour est celle de base si aucune action
        int baseTurnPosition = 5;

        //Permet de definir la position du heros 0
        if (sideOfPlayer == 0)
        {
            defaultPositionX.Add(12000);
            defaultPositionY.Add(5000);
        }
        else
        {
            defaultPositionX.Add(6000);
            defaultPositionY.Add(4000);
        }

        // game loop, start here /!\
        while (true)
        {
            turnCount = turnCount + 1;

            inputs = Console.ReadLine().Split(' ');
            int myHealth = int.Parse(inputs[0]); // Your base health
            int myMana = int.Parse(inputs[1]); // Ignore in the first league; Spend ten mana to cast a spell

            inputs = Console.ReadLine().Split(' ');
            int oppHealth = int.Parse(inputs[0]);
            int oppMana = int.Parse(inputs[1]);

            int entityCount = int.Parse(Console.ReadLine()); // Amount of heros and monsters you can see

            List<Entity> myHeroes = new List<Entity>(entityCount);
            List<Entity> oppHeroes = new List<Entity>(entityCount);
            List<Entity> monsters = new List<Entity>(entityCount);

            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int id = int.Parse(inputs[0]); // Unique identifier
                int type = int.Parse(inputs[1]); // 0=monster, 1=your hero, 2=opponent hero
                int x = int.Parse(inputs[2]); // Position of this entity
                int y = int.Parse(inputs[3]);
                int shieldLife = int.Parse(inputs[4]); // Ignore for this league; Count down until shield spell fades
                int isControlled = int.Parse(inputs[5]); // Ignore for this league; Equals 1 when this entity is under a control spell
                int health = int.Parse(inputs[6]); // Remaining health of this monster
                int vx = int.Parse(inputs[7]); // Trajectory of this monster
                int vy = int.Parse(inputs[8]);
                int nearBase = int.Parse(inputs[9]); // 0=monster with no target yet, 1=monster targeting a base
                int threatFor = int.Parse(inputs[10]); // Given this monster's trajectory, is it a threat to 1=your base, 2=your opponent's base, 0=neither
                Entity entity = new Entity(
                    id, type, x, y, shieldLife, isControlled, health, vx, vy, nearBase, threatFor
                );

                switch (type)
                {
                    case TYPE_MONSTER:
                        monsters.Add(entity);
                        break;
                    case TYPE_MY_HERO:
                        myHeroes.Add(entity);
                        break;
                    case TYPE_OP_HERO:
                        oppHeroes.Add(entity);
                        break;
                }
            }

            //Valeur ajouté pour les deplacements en debut de partie
            int addValueOnX = 0; //Deplacement en X
            int addValueOnY = 0; //Deplacement en Y
            for (int i = 0; i < heroesPerPlayer; i++)
            {

                if (i == 1 && turnCount <= baseTurnPosition)
                { //1er héros
                    addValueOnX = (sideOfPlayer == 0) ? baseX + 2000 : baseX - 2000;
                    addValueOnY = 0;
                }
                else if (i == 2 && turnCount <= baseTurnPosition)
                { //2eme héros
                    addValueOnX = 0;
                    addValueOnY = (sideOfPlayer == 0) ? baseY + 2000 : baseY - 2000;
                }
                else
                { //3eme héros
                    addValueOnX = 0;
                    addValueOnY = 0;
                }

                //Permet de savoir quel est le monstre le plus proche du heros.
                int previousPositionBetweenMonsterAndHero = 0; //Pour comparer à la valeur optimale
                int positionBetweenMonsterAndHero = 0;

                //Permet de savoir quel est le monstre le plus proche du "coeur" de la base.
                int previousPositionBetweenMonsterAndBase = 0; //Pour comparer à la valeur optimale
                int positionBetweenMonsterAndBase = 0;

                //Savoir combien d'ennemi sont proche du héros, utile pour le héros qui cherche a projeter beaucoup d'ennemi en 1 spell
                int numberMonsterAroundHero = 0;

                Entity target = null;

                //Si il y a un monstre ou plus sur la map.
                if (monsters.Count > 0)
                {

                    //Verifie chaque monstres
                    foreach (Entity monster in monsters)
                    {

                        //Si un monstre est dans la zone critique de notre base
                        //Focus sur le monstre le plus proche du "coeur" de la base
                        //4500 ; 4500 pour base bleu / 13130 ; 4500 pour base rouge
                        if (((monster.X <= 4500 && monster.Y <= 4500 && sideOfPlayer == 0) || (monster.X >= 13130 && monster.Y >= 4500 && sideOfPlayer == 1)) && i != 0)
                        {


                            stylePlay = 1; //Passe en mode "jeu defensif"

                            List<object> list = compareDistanceBetweenMonsterAndBase(monster, previousPositionBetweenMonsterAndBase, positionBetweenMonsterAndBase, sideOfPlayer);

                            if ((Entity)list[0] != null)
                            {
                                target = (Entity)list[0];
                            }

                            previousPositionBetweenMonsterAndBase = (int)list[1];
                        }

                        //Si aucun ennemi dans la zone "critique" de la base allié
                        //Focus sur l'ennemi le plus proche de chaque héros dans un rayon de 2500 ; 2500
                        if ((myHeroes[i].X - 2500 <= monster.X && monster.X <= myHeroes[i].X + 2500) && (myHeroes[i].Y - 2500 <= monster.Y && monster.Y <= myHeroes[i].Y + 2500) && previousPositionBetweenMonsterAndBase == 0)
                        {

                            if ((myHeroes[i].X - 1000 <= monster.X && monster.X <= myHeroes[i].X + 1000) && (myHeroes[i].Y - 1000 <= monster.Y && monster.Y <= myHeroes[i].Y + 1000))
                            {
                                numberMonsterAroundHero += 1;
                            }


                            stylePlay = 0; //Passe en mode "jeu normal / offensif"

                            List<object> list = compareDistanceBetweenMonsterAndHeros(monster, myHeroes[i], previousPositionBetweenMonsterAndHero, positionBetweenMonsterAndHero, sideOfPlayer);

                            if ((Entity)list[0] != null)
                            {
                                target = (Entity)list[0];
                            }

                            previousPositionBetweenMonsterAndHero = (int)list[1];

                        }
                    }
                }

                //Deplacement au début de la partie a une position spécifique
                if (turnCount == baseTurnPosition) 
                {
                    if (i != 0)
                    {
                        //Ajoute les positions à des tableaux, pour pouvoir se repositionner.
                        defaultPositionX.Add(myHeroes[i].X);
                        defaultPositionY.Add(myHeroes[i].Y);
                    }
                }

                //Stade avancé de la partie, rapproche le héros 0 de la base ennemi.
                if (turnCount == 130)
                {
                    if (sideOfPlayer == 0)
                    {
                        defaultPositionX[0] = defaultPositionX[0] + 2000;
                        defaultPositionY[0] = defaultPositionY[0] + 2000;
                    }
                    else
                    {
                        defaultPositionX[0] = defaultPositionX[0] - 2000;
                        defaultPositionY[0] = defaultPositionY[0] - 2000;
                    }
                }

                //Verifie si des héros ennemi sont proche du hero 0 à partir du tour 130
                //(Environ le tour a partir duquel l'ennemi commence a se retrouver submerger d'ennemi)
                if (turnCount >= 130 && i == 0)
                {
                    //On parcours les héros ennemis pour savoir si il y a un héros ennemi proche du héros dans la base ennemi
                    foreach (Entity opp in oppHeroes)
                    {
                        if ((myHeroes[i].X - 2500 <= opp.X && opp.X <= myHeroes[i].X + 2500) && (myHeroes[i].Y - 2500 <= opp.Y && opp.Y <= myHeroes[i].Y + 2500))
                        {
                            target = opp;
                        }
                    }
                }

                if (target != null) //Si le héros a une target (monstre dans son périmètre ou monstre dans la zone critique de la base)
                {

                    bool castSpell_OneTime = false; //Empecher qu'un héros fasse plusieurs fois un SPELL en 1 tour

                    //Cast un CONTROL si mana à disposition, la cible ne focus personne / pas notre base et si la cible dispose d'au moins 8 HP.
                    if ((target.ThreatFor == 0 || target.ThreatFor == 1) && myMana >= 30 && target.Health > 8 && castSpell_OneTime == false && stylePlay == 0 && i != 0)
                    {
                        if (sideOfPlayer == 0)
                        {
                            Console.WriteLine($"SPELL CONTROL {target.Id} {18000} {8000}"); //Direction de la base rouge
                        }
                        else
                        {
                            Console.WriteLine($"SPELL CONTROL {target.Id} {0} {0}"); //Direction de la base bleu
                        }
                        castSpell_OneTime = true;
                    }

                    //Permet de pouvoir utiliser WIND quand un monstre et un héros sont dans le coin de la base. (mode de jeu defensif)
                    else if (stylePlay == 1 && sideOfPlayer == 0 && (myHeroes[i].X < 1500 && myHeroes[i].Y < 1500 && target.X < 1200 && target.Y < 1200))
                    {
                        Console.WriteLine($"SPELL WIND {18000} {8000}");
                    }
                    else if (stylePlay == 1 && sideOfPlayer == 1 && (myHeroes[i].X < baseX - 50 && myHeroes[i].Y < baseY - 50 && target.X > baseX - 1500 && target.Y > baseY - 1500))
                    {
                        Console.WriteLine($"SPELL WIND {0} {0}");
                    }


                    //Dirige le héros vers la cible, tant qu'elle n'est pas trop loin dans la base ennemi
                    else if (((sideOfPlayer == 0 && target.X <= 8000) || (sideOfPlayer == 1 && target.X >= 9630)) && castSpell_OneTime == false && target.ThreatFor != 2 && i != 0)
                    {
                        Console.WriteLine($"MOVE {target.X} {target.Y}");
                    }


                    //CONTROL un Héros ennemi pour le repousser vers le centre du terrain
                    else if (target.NearBase == -1 && target.ThreatFor == -1 && castSpell_OneTime == false && myMana >= 30)
                    {
                        Console.WriteLine($"SPELL CONTROL {target.Id} {8000} {6000}");
                        castSpell_OneTime = true;
                    }


                    //Pousse les ennemis si plusieurs ennemi proche du héros qui est proche de la base ennemi
                    else if (i == 0 && (sideOfPlayer == 0 && target.X >= 10000) && target.Y >= 3000)
                    {
                        if (numberMonsterAroundHero >= 2 && myMana >= 30)
                        {
                            Console.WriteLine($"SPELL WIND {18000} {8000}");
                        }
                        else
                        {
                            Console.WriteLine($"MOVE {target.X} {target.Y}");
                        }
                    }
                    else if (i == 0 && (sideOfPlayer == 1 && target.X <= 8000 && target.Y <= 6000))
                    {
 
                        if (numberMonsterAroundHero >= 2 && myMana >= 30)
                        {
                            Console.WriteLine($"SPELL WIND {0} {0}");
                        }
                        else
                        {
                            Console.WriteLine($"MOVE {target.X} {target.Y}");
                        }
                    }

                    //Deplace le héros à sa position initiale
                    else
                    {
                        Console.WriteLine($"MOVE {defaultPositionX[i]} {defaultPositionY[i]}");
                    }
                }
                else
                {
                    //Phase de positionnement
                    if (turnCount < baseTurnPosition && i != 0)
                    {
                        if (sideOfPlayer == 0)
                        {
                            Console.WriteLine($"MOVE {myHeroes[i].X * 2 + addValueOnX} {myHeroes[i].Y * 2 + addValueOnY}");
                        }
                        else
                        {
                            Console.WriteLine($"MOVE {(int)(myHeroes[i].X / 16 + addValueOnX)} {(int)(myHeroes[i].Y / 8 + addValueOnY)}");
                        }
                    }
                    //Deplace à sa position initiale
                    else
                    {
                        Console.WriteLine($"MOVE {defaultPositionX[i]} {defaultPositionY[i]}");
                    }
                }
            }
            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");
        }
    }
}