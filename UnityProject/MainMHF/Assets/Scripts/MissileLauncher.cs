using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GalacticWar
{
    public class MissileLauncher : MonoBehaviour
    {
        public bool mIsAttacking = false;
        public Vector3 mTarget;
        public Sc_Projectile projectile;
        public PlanetComponent mPlanet;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (mIsAttacking)
            {
                Instantiate(projectile, transform.position, Quaternion.identity);
                projectile.mPlanet = mPlanet.gameObject;
                projectile.mPlanetRadius = mPlanet.planetRadius;
                projectile.mGravityAccel = -1.0f;
                Quaternion targetRot = Sc_Utilities.GetCourse(transform, Sc_SphericalCoord.FromCartesian(transform.position).ToGeographic(), Sc_SphericalCoord.FromCartesian(mTarget).ToGeographic());
                projectile.mVelocity = (targetRot * Vector3.forward) * 100.0f + (transform.position - mPlanet.transform.position).normalized * 10.0f;
                mIsAttacking = false;
            }
        }
    }
}