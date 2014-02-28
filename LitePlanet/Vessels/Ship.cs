using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using LiteEngine.Core;
using LiteEngine.Rendering;
using LiteEngine.Textures;
using LiteEngine.Math;
using LiteEngine.Physics;
using LiteEngine.Particles;
using FarseerPhysics.Common;
using FarseerPhysics.Factories;
using FarseerPhysics.Dynamics;
using LitePlanet.Effects;
using LitePlanet.Weapons;
using LitePlanet.Projectiles;
using LitePlanet.Worlds;

namespace LitePlanet.Vessels
{
    class Ship : IPhysicsObject, IDamageSink, ICollisionField
    {
        Cannon _cannon;
        Body _body;
        static Texture _texture = new Texture("rocketship");
        static Texture _redTexture = new Texture("redship");
        static Texture _circleTexture = new Texture("circleOverlay");
        static Texture _triangleTexture = new Texture("triangleOverlay");

        protected float _engineMaxThrust = 5f;

        protected Engine _engine;
        bool _hostile;
        protected float _maxSpeed = 100;
        public Ship(Engine engine, bool other=false)
        {
            _engine = engine;
            _cannon = new Cannon(10);
            _body = CreateBody();
            _hull = 100;
            if (other)
                _hostile = true;
            if (_hostile)
                _hull = 100;
        }

        protected virtual Body CreateBody()
        {
            Body body = _engine.Physics.CreateBody(this);
            body.BodyType = BodyType.Dynamic;
            body.AngularDamping = 0.5f;
            body.Friction = 0f;
            body.Restitution = 1.1f;
            body.Mass = 0.5f;
            body.Rotation = 0f;
            FixtureFactory.AttachPolygon(new Vertices(new Vector2[] { new Vector2(0f, -0.4f), new Vector2(0.35f, 0.4f), new Vector2(-0.35f, 0.4f) }), 1f, body);
            body.CollisionCategories = Category.Cat2;
            body.CollidesWith = Category.Cat1 | Category.Cat2 | Category.Cat3;
            return body;
        }

        public Body Body
        {
            get
            {
                return _body;
            }
        }

        public Cannon PrimaryWeapon
        {
            get
            {
                return _cannon;
            }
        }


        protected int _hull = 300;
        public int Hull
        {
            get
            {
                return _hull;
            }
        }

        float _jumpCharge = 0;
        public float JumpDriveCharge
        {
            get
            {
                return _jumpCharge;
            }
        }

        public bool JumpDriveCharging
        {
            get
            {
                return _jumpDriveOn;
            }
        }

        bool _jumpDriveOn = false;
        public void Jump(bool on)
        {
            _jumpDriveOn = on;
        }

        int _fuel = 5500;
        public int Fuel
        {
            get
            {
                return _fuel;
            }
        }

        public Vector2 Position
        {
            get 
            {
                return _body.Position;
            }
            set
            {
                _body.Position = value;
            }
        }

        public Vector2 Velocity
        {
            get 
            {
                return _body.LinearVelocity;
            }
        }

        public Vector2 Facing
        {
            get 
            {
                return _body.GetWorldVector(new Vector2(0, -1));
            }
        }

        public float Rotation
        {
            get 
            {
                return _body.Rotation;
            }
            set
            {
                _body.Rotation = value;
            }
        }


        protected float _enginePercent;
        float _engineTargetPercent;
        internal void ActivateEngines(float percent=1)
        {
            _engineTargetPercent = percent;
            if (_body.Mass < 1)
                _enginePercent = 1;
        }

        int s = 0;
        public void ApplyForwardThrust(float amount)
        {
            _body.ApplyForce(Facing * amount);
            float len = _body.LinearVelocity.LengthSquared();

            if (len > _maxSpeed)
                _body.LinearVelocity *= _maxSpeed / len;
        }

        public void ApplyRotateThrust(float amount)
        {
            _body.ApplyTorque(amount);
        }

        public virtual void Draw(XnaRenderer renderer)
        {
            if (_hull <= 0)
                return;
            Texture texture = _texture;
            if (_hostile)
                texture = _redTexture;
            renderer.DrawSprite(texture, Position, new Vector2(1f, 1f), Rotation);

            if (renderer.Camera.Zoom > 2)
            {
                float width = renderer.Camera.Zoom;

                Color c = Color.FromNonPremultiplied(new Vector4(0,1,0,1));
                if (_hostile)
                    c = Color.Red;
                renderer.DrawSprite(_triangleTexture, Position, new Vector2(width, width), Rotation, c, 0f);
            }
        }

        public int Gold = 0;

        public void OnCollideWith(IPhysicsObject self, IPhysicsObject other, float impulse)
        {
            if (other is Item)
            {
                Item item = other as Item;
                item.Remove();
                Gold++;
                return;
            }

            TakeDamage((int)Math.Pow(impulse * 2, 2));

            //IDamageSink sink = other as IDamageSink;
            //sink.TakeDamage((int)impulse * 2);
        }

        public void TakeDamage(int damageAmount)
        {
            _hull -= damageAmount;
            if (_hull < 0)
            {
                _hull = 0;
                _fuel = 0;
                Explosion explosion = new Explosion(_engine);
                explosion.Create(Position);
                //_engine.Physics.RemoveBody(_body);
            }
        }

        Vector2 ICollisionField.Position
        {
            get 
            {
                return Position;
            }
        }

        int ICollisionField.Size
        {
            get 
            {
                return 25;
            }
        }

        internal void Update()
        {
            if (_enginePercent < _engineTargetPercent)
                _enginePercent = Math.Min(_enginePercent + 0.01f, _engineTargetPercent);
            else
                _enginePercent = Math.Max(_enginePercent - 0.01f, 0);
            float thrust = _engineMaxThrust * _enginePercent;

            if (_fuel <= 0)
                return;
            _fuel -= (int)thrust;
            if (_fuel < 0)
                _fuel = 0;

            ApplyForwardThrust(thrust);

            Vector2 vel = Velocity - Facing * 5.1f;
            vel.X += Dice.Next() * 1.6f - 0.8f;
            vel.Y += Dice.Next() * 1.6f - 0.8f;

            if (_body.Mass > 1)
                return;
            Vector2 enginePosition = Position - Facing * 0.6f;
            float power = _enginePercent;
            if (Dice.Next() < power)
            {
                Particle exhaust = _engine.ExhaustParticles.CreateParticle(enginePosition, vel, 50);
                if (exhaust == null)
                    return;
                exhaust.Body.CollidesWith = Category.Cat1;
                exhaust.Body.CollisionCategories = Category.Cat6;
            }

            _engineTargetPercent = 0;
            _enginePercent = 0;
            //Vector2 p = enginePosition + Dice.RandomVector(0.3f); // Position - Facing * 0.7f 
            //_engine.SmokeParticles.CreateParticle(p, Velocity * 0, 50);
        }
    }
}
