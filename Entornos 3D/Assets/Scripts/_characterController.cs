using UnityEngine;

public class _characterController : MonoBehaviour
{
    public Camera cam;
    private InputData input;
    public AnimMovement characterMovement;

    // Start is called before the first frame update
    void Start()
    {
        characterMovement = GetComponent<AnimMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        // Get input from player
        input.getInput();

        //Apply input to character
        CharacterMovement();
    }
  
    void CharacterMovement()
    {
        float h = input.Horizontal;
        float v = input.Vertical;
        Camera c = cam;

        bool j = input.jump;

        bool d = input.dash;



        characterMovement.moveCharacter(h, v, c, j, d);

    }


}
