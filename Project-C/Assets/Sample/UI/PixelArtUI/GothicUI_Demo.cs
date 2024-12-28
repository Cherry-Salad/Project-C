/*
 * Author: Dimitrios Gkaltsidis
 * Date: 16 March 2024
 * Disclaimer: This code is not fully optimized. For production-level 2D character functionality, consider crafting your own.
 * Version: 1.0.0
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GothicUI_Demo : MonoBehaviour
{
    [Header("BIG UI FILLER IMAGES")]
    public Image enemyHpImageBig;
    public Image playerHpImageBig;
    public Image playerStaminaImageBig;
    public Image playerManaImageBig;

    [Header("SMALL UI FILLER IMAGES")]
    public Image enemyHpImageSmall;
    public Image playerHpImageSmall;
    public Image playerStaminaImageSmall;
    public Image playerManaImageSmall;

    [Header("SLIDERS")]
    public Slider enemyHpSlider;
    public Slider playerHpSlider;
    public Slider playerStaminaSlider;
    public Slider playerManaSlider;

    void Start()
    {
        // Makes all the HP, Stamina and Mana values max
        enemyHpSlider.value = 100f;
        playerHpSlider.value = 100f;
        playerStaminaSlider.value = 100f;
        playerManaSlider.value = 100f;
    }

    // Update is called once per frame
    void Update()
    {
        // Update enemy HP
        enemyHpImageBig.fillAmount = enemyHpSlider.value;
        enemyHpImageSmall.fillAmount = enemyHpSlider.value;

        // Update player HP
        playerHpImageBig.fillAmount = playerHpSlider.value;
        playerHpImageSmall.fillAmount = playerHpSlider.value;

        // Update player stamina
        playerStaminaImageBig.fillAmount = playerStaminaSlider.value;
        playerStaminaImageSmall.fillAmount = playerStaminaSlider.value;

        // Update player mana
        playerManaImageBig.fillAmount = playerManaSlider.value;
        playerManaImageSmall.fillAmount = playerManaSlider.value;
    }
}
