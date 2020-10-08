﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Global;
using MyProg;
using System;


public enum blockMaterial
{
    all= 0,
    wood=1,
    ground=2,
    glass=3,
    metal=4,
    badrock=5,
    meat = 6,
    stone = 7,
}


public enum blockType
{
    none = 0,
    cargo = 1,
    door = 2,
    mobSpawn = 3,
    agregat = 4,
}

public class BlockController : MonoBehaviour
{
    public string itemInd = "def:not";
    public itemSave myData;
    public blockType myType;
    public blockMaterial myMaterial;

    public float hpMax = 9f;
    public float hp = 8f;



    public string dropInd = "";



    public GameObject myCrackEffect;
    public GameObject myCrackEffectEasy;

    // Start is called before the first frame update
    void Start()
    {

    }



    //Вернуть дроп
    public string getDrop()
    { 
        return dropInd;
    }

    
   

    public void crackUpdate()
    {
        myCrackEffectEasy.SetActive(hp < hpMax * 0.8f);
        myCrackEffect.SetActive(hp < hpMax * 0.5f);
    }

    public void sosedBuild()
    {


        initBlock();

        return;

        Vector3 location = transform.position + new Vector3(1, 0, 0);
       
        
        RaycastHit[] hits = Physics.BoxCastAll(transform.position, transform.localScale*2f, Vector3.up*-0.2f,transform.rotation,3f);
        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
             
                if (hits[i].transform.tag == "block")
                {
                hits[i].transform.GetComponent<BlockController>().initBlock();


                }
        }

        
        
    /*

    RaycastHit hit;
        if (Physics.BoxCast(transform.position, transform.forward*2f, out hit))
        {
            Debug.Log("Point of contact: " + hit.point);
            if(hit.transform.tag == "block")
            {
                hit.transform.GetComponent<BlockController>().initBlock();


            }
        }
        */
 
    }

    public void Damage(float dam, blockMaterial attackMaterial = blockMaterial.all, GameObject author = null)
    {

        sosedBuild();

        float cofDamage = 1f;

        if (attackMaterial != myMaterial)
        {
            cofDamage = 0.4f;
            if (attackMaterial == blockMaterial.all) cofDamage = 1f;
            if (attackMaterial == blockMaterial.meat) cofDamage = 0.3f;
            if (myMaterial == blockMaterial.glass) cofDamage = 1f;
            if (myMaterial == blockMaterial.badrock) cofDamage = 0f;
            if (myMaterial == blockMaterial.all) cofDamage = 1f;
            if (myMaterial == blockMaterial.metal && attackMaterial == blockMaterial.stone) cofDamage = 1f;
        }

        dam = dam * cofDamage;
        hp -= dam;

        if (hp > hpMax)
        {
            hp = hpMax;
        }

        crackUpdate();



        if (hp <= 0f)
        {
            if (author != null)
            {
                string _drop = getDrop();
                if (_drop != "" && _drop != "not")
                {
                    author.GetComponent<PlayerAction>().myInv.myData.itemAdd(_drop, 1);
                }
            }
            //transform.parent.GetComponent<ChankController>();
            Destroy(gameObject);
        }
    }





    void createBuild(string iniFilePath)
    {
        IniFile MyIni = new IniFile(iniFilePath);



        int buildCount = MyIni.ReadInt("buildCount", "build", 0);

        for(int i=0; i< buildCount; i++)
        {

            string line = MyIni.Read("b" + i.ToString(), "build", "not");
            if (line != "not")
            {
               // print("Ошибка чтения билда: " + i.ToString());
              //  return;


                string[] conf = line.Split(' ');

                 
                string[] posBlock = conf[0].Split(':');

                Vector3 newPos = new Vector3(transform.localPosition.x + System.Convert.ToInt32(posBlock[0]), 1 + transform.localPosition.y + System.Convert.ToInt32(posBlock[1]), transform.localPosition.z + System.Convert.ToInt32(posBlock[2]));


                BlockController b = Instantiate(transform.parent.GetComponent<ChankController>().mapCon.blockBlank, transform.parent);

                if (transform.parent.Find(Global.Links.vectorToString(newPos))!=null)
                {
                    Destroy(transform.parent.Find(Global.Links.vectorToString(newPos)).gameObject);
                }


                b.transform.localPosition = newPos;

//                b.transform.name = b.transform.localPosition.x.ToString() + ":" + b.transform.localPosition.y.ToString() + ":" + b.transform.localPosition.z.ToString();
                b.transform.name = Global.Links.vectorToString(b.transform.localPosition);


                b.itemInd = conf[1];
                b.hp = 0;


                itemBlockSettings myBlockSetting = mod.blockBase[b.itemInd];
                b.myBlockSetting = myBlockSetting;
                b.initBlockPreload();

               // b.initBlock();



            }


        }

        Destroy(gameObject);
    }


    public void createCargo()
    {
        invData myInv = gameObject.AddComponent<invData>();



        myInv.init();
        //myInv.itemAdd("Weapon:arrow");
        // myInv.itemAdd("Weapon:arrow");


        if (itemInd != "Chest:adminCargo")
        {

            //myInv.dataSet();


        }


        if (itemInd == "Chest:adminCargo")
        {
            //  print("Admin chest");

            foreach (KeyValuePair<string, itemSave> item in Global.Links.getModLoader().itemBase)
            {
                myInv.itemAdd(item.Key);

            }


        }

    }


    public ModLoader mod;

    public itemBlockSettings myBlockSetting;




    public void initBlockPreload()
    {
        if (mod == null)
        {
            mod = Global.Links.getModLoader();
        }



        if (myBlockSetting == null)
        {
            myBlockSetting = mod.blockBase[itemInd];
        }


        if (isInit) return;
        if (isInitPreload) return;
        isInitPreload = true;


        GetComponent<MeshRenderer>().material.mainTexture = myBlockSetting.icon;


        if (myBlockSetting.buildCount > 0)
        {
            initBlock();
        }


        hpMax = myBlockSetting.hpMax;
        if (hp <= 0)
        {
            hp = hpMax;
        }
        else
        {
            if (hp < hpMax)
            {
                crackUpdate();
            }
        }

        

    }



    bool isInitPreload = false;
     bool isInit = false;
    public void initBlock()
    {
        if (isInit) return;
        isInit = true;
        isInitPreload = true;



        if (myBlockSetting == null)
        {
            print("ERROR LOAD");
            return;
        }

            if (itemInd == string.Empty) itemInd = "Core:dirt";
        if (itemInd == "def:not") itemInd = "Core:dirt";
        if (itemInd == "not") itemInd = "Core:dirt";


       


        if (!mod.blockBase.ContainsKey(itemInd))
        {
            itemInd = "Core:dirt";
        }

       
        myData = mod.itemBaseGetFromInd(itemInd);

        if (myData == null)
        {
            return;
        }

        /*
        var watch = System.Diagnostics.Stopwatch.StartNew();
        watch.Stop();
        print($"Execution Time: {watch.ElapsedMilliseconds} ms");
        */

        // IniFile MyIni = new IniFile(myData.iniFilePath);





        





        myMaterial = myBlockSetting.material;
        myType = myBlockSetting.type;

  

        dropInd = myBlockSetting.dropInd;

    

        if (myBlockSetting.width != 1f)
        {
            transform.localScale = new Vector3(myBlockSetting.width, myBlockSetting.height, myBlockSetting.width);
        }


      //  GetComponent<MeshRenderer>().material.mainTexture = myData.icon;
         

 
        //Генерация строений
        int buildCount = myBlockSetting.buildCount;// MyIni.ReadInt("buildCount", "build", 0);
        if (buildCount > 0)
        {
            createBuild(myData.iniFilePath);
        }


        if (myType == blockType.cargo)
        {
            createCargo();
        }



        if (myType == blockType.mobSpawn)
        {
            gameObject.AddComponent<blockTypeSpawner>().init();
             
        }


        if (myType == blockType.agregat)
        {
            gameObject.AddComponent<blockTypeAgregat>().init();

        }

        if (itemInd == "Core:adminBuild")
        {
            gameObject.AddComponent<adminBuilderBlock>().createAdminBuildBlockInit();

        }

    }



}