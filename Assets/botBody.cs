﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;






[RequireComponent(typeof(CharacterController))]
public class botBody : MonoBehaviour
{
    public AudioSource myAudio;

    packSound myPackSound;
    
        Animator anim;
   [Header("Blank Prefabs")]
    public GameObject pref_bulletHole;


    [Header("Ai")]
    public float distAttack = 2f;
    public float distView = 12f;
    public bool gunIsset = false;
    public float hp = 1f;
    public float lerpSpeed = 0.3f;
    public float lerpSpeedHand = 15f;
    public int jumpRandomFrom = 1200;


    [Header("Attack")]
    public float timerAttackMax = 0.9f;
    public float attackDamage = 1f;
    public float fireRayDist = 2f;


    float timerAttack;


    [Header("Body Part")]
    public Transform handCenter;


    [Header("Body")]
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;





    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;



    public void animDeadEnd()
    {
        
        // print("ANIM END");
        Destroy(gameObject);
    }

    [HideInInspector]
    public float rotationX = 0;
    public bool canMove = true;

    bool isDeath = false;
    public void Damage(float val)
    {
        if (isDeath) return;



        myAudio.PlayOneShot(myPackSound.getSound(myPackSound.souDamage));

        hp -= val;
        if (hp <= 0f)
        {
            canMove = false;
            isDeath = true;
           anim.SetBool("isDeath", true);

        }
    }

    void Start()
    {
        anim = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
 
        target = GameObject.Find("Player");

        sUi sui = Global.Links.getSui();
        myPackSound = sui.GetComponent<packSound>();

        myAudio = GetComponent<AudioSource>();
    }


    public void jump()
    {
        if (canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
    }


    public GameObject target;

    public void gunFire()
    {


        Vector3 deltaVec = target.transform.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(deltaVec);


        Vector3 forward = transform.forward;

        forward = rotation.ToEulerAngles();

        forward = handCenter.transform.forward;

        forward += new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.8f, 0.2f), Random.Range(-0.2f, 0.2f));


        Vector3 rayOrigin = transform.Find("start").transform.position;

        RaycastHit hit;

        // Set the start position for our visual effect for our laser to the position of gunEnd


        float distRay = fireRayDist;

        if (gunIsset)
        {

            myAudio.PlayOneShot(myPackSound.getSound(myPackSound.souGunFire));
            distRay = 15f;
        }
        else
        {

            //myAudio.PlayOneShot(myPackSound.getSound(myPackSound.sou));
        }

        //Debug.DrawRay(rayOrigin, forward * 25f, Color.red, 5f);

        // Check if our raycast has hit anything
        if (Physics.Raycast(rayOrigin + forward * 0.2f, forward, out hit, distRay))
        {

            Transform damTo = hit.transform;




            if (hit.transform.tag == "Untagged")
            {
                damTo = hit.transform.parent;
                if (damTo.transform.tag == "Untagged")
                {
                    return;
                }
            }




            if (damTo.transform.tag == "block")
            {
                BlockController b = damTo.GetComponent<BlockController>();
                myAudio.PlayOneShot(myPackSound.getDigSound(b.myMaterial));

                b.Damage(attackDamage*0.55f, blockMaterial.all, null);
                return;
            }

            if (damTo.transform.tag == "meatPart")
            {
                damTo.transform.GetComponent<meatPart>().Damage(attackDamage, gameObject);

            }

            if (damTo.transform.tag == "Player")
            {
                damTo.GetComponent<PlayerAction>().Damage(attackDamage);
            }
            else
            {
                GameObject bulHole = Instantiate(pref_bulletHole, damTo.transform);

                bulHole.transform.position = hit.point;
                bulHole.transform.LookAt(transform.position, -Vector3.up);
            }



        }

    }

    float timeNoPlayer = 0f;

    void Update()
    {
        float moveForw = 0f;

        bool isRunning = false;

        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        // Press Left Shift to run

        if (!isDeath)
        {




            timeNoPlayer += Time.deltaTime;
            if (timeNoPlayer > 60f * 1.5f)
            {
                print("Моб выпилил себя, потому что не смог до вас добравться");
                Destroy(gameObject);

            }




            anim.SetBool("isJump", !characterController.isGrounded);






            Vector3 lTargetDir = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z) - transform.position;
            lTargetDir.y = 0.0f;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lTargetDir), Time.time * lerpSpeed);



            lTargetDir = target.transform.position + Vector3.up * 1f - handCenter.transform.position;

            handCenter.transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lTargetDir), Time.time * lerpSpeedHand);



            timerAttack -= Time.deltaTime;
            if (timerAttack <= 0f)
            {
                timerAttack = timerAttackMax;

                if (Vector3.Distance(transform.position, target.transform.position) < 10f)
                {
                    timeNoPlayer = 0f;
                    gunFire();
                }

            }







            if (Random.Range(1, 1200) < 2)
            {
                myAudio.PlayOneShot(myPackSound.getSound(myPackSound.souZomb));
            }


            if (Vector3.Distance(transform.position, target.transform.position) > distAttack)
            {
                moveForw = 1f;

                if (Random.Range(1, jumpRandomFrom) < 2)
                {

                    jump();
                }

            }
            else
            {

                moveForw = 0f;
            }



            if (moveForw > 0f)
            {
                if (Random.Range(1, 50) < 2)
                {

                    myAudio.PlayOneShot(myPackSound.getSound(myPackSound.souStep));
                }
            }

        }

        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * moveForw : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * 0f : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);


        moveDirection.y = movementDirectionY;


        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);


        if (transform.position.y < -6f)
        {
            Destroy(gameObject);
        }
    }
}