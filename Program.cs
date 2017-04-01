using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace StarterKit
{
	public class Object
	{
		public static List<Object> ObjectsList = new List<Object>();
		public double ShotSpeed = 0.1;
		public float health = 100;
		public double radius = 1;
		public double angle;
		public double x = 0;
		public double y = 0;
		public double speedx = 0;
		public double speedy = 0;
		public bool player = false;
		public bool MustDie;
		//public int index = -1;
		public Object(bool p = false)
		{
			//ObjectsList.Add(this);
			bool inserted = false;
			for(int i = 1; i < ObjectsList.Count; i++)
			{
				if(ObjectsList[i] == null)
				{
					ObjectsList[i] = this;
					//index = i;
					inserted = true;
					break;
				}
			}
			if(!inserted)
			{
				ObjectsList.Add(this);
				//index = ObjectsList.Count - 1;
			}
			player = p;
		}
		public bool TakeDamage(float damage)
		{
			health -= damage;
			if(health <= 0f)
			{
				Die();
				return true;
			}
			return false;
		}
		public void SetFacingTarget(int tx, int ty)
		{
			Vector2 vec = Game.game.MouseToWorldCoordinates(tx, ty);
        	double dx = vec.X - x;
            double dy = vec.Y - y;
            SetFacingDirection(dx, dy);
		}
		public void SetFacingDirection(double dx, double dy)
		{
			if((dx < double.Epsilon) && (dx > -double.Epsilon))
            	if(dy > 0)
            		angle = Math.PI/2;
            	else
            		angle = -Math.PI/2;
            else
            {
            	angle = Math.Atan(dy/dx);
            	if(dx < 0)
            		angle += Math.PI;
            }
            angle *= 180/Math.PI;
		}
		public virtual void Update()
		{
			x += speedx*Game.DeltaGameTime;
        	y += speedy*Game.DeltaGameTime;
        	if(player)
        	{
        		/*Vector2 vec = Game.game.MouseToWorldCoordinates(Game.game.MouseX, Game.game.MouseY);
        		double dx = vec.X - x;
            	double dy = vec.Y - y;
            	if(dx == 0)
            		if(dy > 0)
            			angle = Math.PI/2;
            		else
            			angle = -Math.PI/2;
            	else
            	{
            		angle = Math.Atan(dy/dx);
            		if(dx < 0)
            			angle += Math.PI;
            	}
            	angle *= 180/Math.PI;*/
        		SetFacingTarget(Game.game.MouseX, Game.game.MouseY);
        	}
		}
		public void Shoot(double speedx, double speedy)
		{
			new Bullet(this,this.x, this.y, speedx, speedy);
		}
		public void Die()
		{
			MustDie = true;
		}
	}
	
	public class Enemy : Object
	{
		double LastShotTime= 0;
		double CooldownTime = 1000;
		double LastSpeedChangeTime;
		double SpeedChangeCooldown = 1000;
		public override void Update()//Todo bot
		{
			    //x += speedx;
            	//y += speedy;
            	int playerindex = -1;
            	for(int i = 0; i < ObjectsList.Count; i++)
            		if(ObjectsList[i].player == true)
            	{
            		playerindex = i;
            		break;
            	}
            	if((playerindex >= 0)&&(Game.GameTime - LastShotTime > CooldownTime))
            	{
            		double sx = ObjectsList[playerindex].x - x;
            		double sy = ObjectsList[playerindex].y - y;
            		double a = Math.Sqrt(sx*sx + sy*sy);
					if(a <= 0)
					{
						sx = 1;
						sy = 1;
					}
					else
					{
						sx /= a;
						sy /= a;
					}
					sx*=ShotSpeed;
					sy*=ShotSpeed;
            		Shoot(sx, sy);
            		LastShotTime = Game.GameTime;
            	}
            	if(Game.GameTime - LastSpeedChangeTime > SpeedChangeCooldown)
            	{
            		speedx = (Game.RNG.NextDouble()-0.5)*0.01;
            		speedy = (Game.RNG.NextDouble()-0.5)*0.01;
            		LastSpeedChangeTime = Game.GameTime;
            	}
            	base.Update();
		}
	}
	
	public class Bullet : Object
	{
		Object Owner;//Todo May prevent objects from destruction this link may keep them alive
		int SpawnTick;
		int lifetime = 1000;
		float Damage = 20;
		public Bullet(Object O = null, double nx = 0, double ny = 0, double nsx = 0, double nsy = 0)
		{
			radius = 0.5;
			health = 1;
			Owner = O;
			x = nx;
			y = ny;
			speedx = nsx;
			speedy = nsy;
			SpawnTick = System.Environment.TickCount;
			SetFacingDirection(speedx, speedy);
			/*int tx = (int)x;
			int ty = (int)y;
			double abssx = speedx;
			if(abssx < 0)
				abssx = - abssx;
			double abssy = speedy;
			if(abssy < 0)
				abssy = - abssy;
			if(abssx > 1 && abssy > 1)
			{
				tx += (int)(speedx);
				ty += (int)(speedy);
			}
			else
			{
				if(abssx < abssy)
					if(abssx < double.Epsilon)
					{
						if(abssy > double.Epsilon)
						{
							if(speedy > 0)
								ty += 1;
							else
								ty += -1;
						}
					}
					else
					{
						tx += (int)(speedx/abssx);
						ty += (int)(speedy/abssx);
					}
				else
					if(abssy < double.Epsilon)
					{
						if(abssx > double.Epsilon)
						{
							if(speedx > 0)
								tx += 1;
							else
								tx += -1;
						}
					}
					else
					{
						tx += (int)(speedx/abssy);
						ty += (int)(speedy/abssy);
					}
			}
			if((int)x != tx || (int)y != ty)
				SetFacingTarget(tx,ty);*/
		}
		public override void Update()
		{
			if(System.Environment.TickCount - SpawnTick > lifetime)
			{
				Die();
				return;
			}
			for(int i = 0; i < ObjectsList.Count; i++)
			{
				if(((x - ObjectsList[i].x)*(x - ObjectsList[i].x) + (y - ObjectsList[i].y)*(y - ObjectsList[i].y)) < (radius + ObjectsList[i].radius)*(radius + ObjectsList[i].radius))
					if((Owner != ObjectsList[i])&&(ObjectsList[i] != this)&&!(ObjectsList[i] is Bullet))
				{
						ObjectsList[i].TakeDamage(Damage);
						Die();
				}
			}
			base.Update();
			//Console.WriteLine(x+" "+y+"\n");
		}
	}
	
    class Game : GameWindow
    {
    	public static Random RNG = new Random();
    	public static double GameTime;
    	public static double DeltaGameTime;
    	public int WPressed{ get; set; }
		public int APressed;
		public int SPressed;
		public int DPressed;
	 	float Zoom = 60f;
		//public bool LeftMouseDown;
		//public int LeftMouseClickX;
		public int LastUpdateTick;
		uint CharacterTexture;   
        uint TerrainTexture;
        uint BulletTexture;
        public int MouseX;
        public int MouseY;
		
        public Game()
            : base(800, 600, GraphicsMode.Default, "OpenTK Quick Start Sample")
        {
            //VSync = VSyncMode.On;
            VSync = VSyncMode.Off;
            //Object.ObjectsList.Add(new Object());
            //new Object(true);
            //Object nO = new Object();
            //nO.x = 10;
            //nO.y = 10;
            new Object(true);
            /*Enemy nO2 = new Enemy();
            nO2.x = 10;
            nO2.y = 0;*/
            /*nO2 = new Enemy();
            nO2.x = 10;
            nO2.y = 10;
            nO2 = new Enemy();
            nO2.x = 5;
            nO2.y = 10;
            nO2 = new Enemy();
            nO2.x = 5;
            nO2.y = 5;
            nO2 = new Enemy();
            nO2.x = 7;
            nO2.y = 5;*/
            //Object.ObjectsList[0].Die();
            //new Bullet(null, -10, 10, 0.01, 0);
            //PlayerObjectIndex = 0;
            LastUpdateTick = System.Environment.TickCount;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GL.ClearColor(1f, 1f, 1f, 0.0f);
            //GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            
            TerrainTexture = (uint)LoadTexture("tex.jpg");
            CharacterTexture = (uint)LoadTexture("Character.png");
            BulletTexture = (uint)LoadTexture("Bullet.png");
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 0.01f, 1000.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
        }
        
        /*protected override void OnUpdateFrame(FrameEventArgs e)
        {
        	base.OnUpdateFrame(e);
        }*/

        protected override void OnRenderFrame(FrameEventArgs e)
        {
        	DeltaGameTime = System.Environment.TickCount - LastUpdateTick;
        	GameTime += DeltaGameTime;
			int playerindex = -1;
			for(int i = 0; i < Object.ObjectsList.Count; i++)
            {
            	Object.ObjectsList[i].Update();
			}
			for(int i = 0; i < Object.ObjectsList.Count; i++)
            {
				if(Object.ObjectsList[i].MustDie)
				{
					Object.ObjectsList.RemoveAt(i);
					i--;
				}
			}
			for(int i = 0; i < Object.ObjectsList.Count; i++)
            {
            	if(Object.ObjectsList[i].player)
            	{
            		playerindex = i;
            		break;
            	}
			}
			if(playerindex >= 0)
			{
				double dT = DeltaGameTime / 20.0;
				if(SPressed > WPressed)
					Object.ObjectsList[playerindex].y-=dT;
				else
					if(WPressed > 0)
						Object.ObjectsList[playerindex].y+=dT;
				if(APressed > DPressed)
					Object.ObjectsList[playerindex].x-=dT;
				else
					if(DPressed > 0)
						Object.ObjectsList[playerindex].x+=dT;
				/*if(LeftMouseDown)
				{
					//Console.WriteLine(OpenTK.Input.Mouse.GetCursorState().X);
					//Console.WriteLine(Object.ObjectsList[playerindex].x);
					double shootx = OpenTK.Input.Mouse.GetCursorState().X - Object.ObjectsList[playerindex].x;
					double shooty = OpenTK.Input.Mouse.GetCursorState().Y - Object.ObjectsList[playerindex].y;
					double a = Math.Sqrt(shootx*shootx + shooty*shooty)*100;
					if(a <= 0)
					{
						shootx = 1;
						shooty = 1;
					}
					else
					{
						shootx /= a;
						shooty /= a;
					}
					Object.ObjectsList[playerindex].Shoot(shootx, shooty);
					//Console.WriteLine("gg");
					LeftMouseDown = false;
				}*/
			}
			LastUpdateTick = System.Environment.TickCount;
            base.OnRenderFrame(e);
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            /*Matrix4 modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);

            GL.Begin(BeginMode.Triangles);

            GL.Color3(1.0f, 1.0f, 0.0f); GL.Vertex3(-1.0f, -1.0f, 4.0f);
            GL.Color3(1.0f, 0.0f, 0.0f); GL.Vertex3(1.0f, -1.0f, 4.0f);
            GL.Color3(0.2f, 0.9f, 1.0f); GL.Vertex3(0.0f, 1.0f, 4.0f);

            GL.End();*/
            Matrix4 modelview;
            if(playerindex >= 0)
            {
            	Vector3 cameraorigin = new Vector3((float)Object.ObjectsList[playerindex].x, (float)Object.ObjectsList[playerindex].y, Zoom);
            	Vector3 cameratarget = new Vector3(cameraorigin.X, cameraorigin.Y, 0);
            	modelview = Matrix4.LookAt(cameraorigin, cameratarget, Vector3.UnitY);
            }
            else
            	modelview = Matrix4.LookAt(Vector3.UnitZ*Zoom, Vector3.Zero, Vector3.UnitY);
            //Matrix4 modelview = Matrix4.LookAt(new Vector3(10,20,60), new Vector3(10,20,0), Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);
            
            GL.BindTexture(TextureTarget.Texture2D, TerrainTexture);
            GL.Begin(BeginMode.Quads);
			//GL.ActiveTexture(TextureUnit.Texture0);
            //GL.Color3(1.0f, 1.0f, 1.0f);
            //GL.BindTexture(TextureTarget.Texture2D, TerrainTexture);
			GL.TexCoord2(0, 0);
			GL.Vertex3(-10.0f, -10.0f, 0.0f);
			GL.TexCoord2(10, 0);
			GL.Vertex3(10.0f, -10.0f, 0.0f);
			GL.TexCoord2(10, 10);
			GL.Vertex3(10.0f, 10.0f, 0.0f);
			GL.TexCoord2(0, 10);
			GL.Vertex3(-10.0f, 10.0f, 0.0f);
            GL.End();
            //GL.Color3(1.0f, 1.0f, 1.0f);
            //GL.BindTexture(TextureTarget.Texture2D, CharacterTexture);
            //GL.ActiveTexture(TextureUnit.Texture1);
            //GL.BindTexture(TextureTarget.Texture2D, CharacterTexture);
            for(int i = 0; i < Object.ObjectsList.Count; i++)
            {
            	GL.PushMatrix();
            	GL.Translate(Object.ObjectsList[i].x, Object.ObjectsList[i].y, 0);
            	GL.Rotate(Object.ObjectsList[i].angle,0,0,1);
            	GL.Translate(-Object.ObjectsList[i].x, -Object.ObjectsList[i].y, 0);
            	if(Object.ObjectsList[i] is Bullet)
            		GL.BindTexture(TextureTarget.Texture2D, BulletTexture);
            	else
            		GL.BindTexture(TextureTarget.Texture2D, CharacterTexture);
            	GL.Begin(BeginMode.Quads);
            	GL.TexCoord2(0, 0); GL.Vertex2(Object.ObjectsList[i].x-Object.ObjectsList[i].radius, Object.ObjectsList[i].y-Object.ObjectsList[i].radius);
            	GL.TexCoord2(1, 0); GL.Vertex2(Object.ObjectsList[i].x+Object.ObjectsList[i].radius, Object.ObjectsList[i].y-Object.ObjectsList[i].radius);
            	GL.TexCoord2(1, 1); GL.Vertex2(Object.ObjectsList[i].x+Object.ObjectsList[i].radius, Object.ObjectsList[i].y+Object.ObjectsList[i].radius);
            	GL.TexCoord2(0, 1); GL.Vertex2(Object.ObjectsList[i].x-Object.ObjectsList[i].radius, Object.ObjectsList[i].y+Object.ObjectsList[i].radius);
            	GL.End();
            	GL.PopMatrix();
            }
            /*GL.Begin(BeginMode.Triangles);
			GL.BindTexture(TextureTarget.Texture2D, Texture);
			GL.TexCoord2(0, 0);
			GL.Vertex3(-1.0f, -1.0f, 4.0f);
			GL.TexCoord2(1, 0);
			GL.Vertex3(1.0f, -1.0f, 4.0f);
			GL.TexCoord2(0.5, 1);
			GL.Vertex3(0.0f, 1.0f, 4.0f);

			GL.End();*/
            
            SwapBuffers();
            //Console.WriteLine(FPS());
        }
        
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
        	if(e.Delta > 0)
        		Zoom--;
        	else
        		if(e.Delta < 0)
        			Zoom++;
        }
        
        public Vector2 MouseToWorldCoordinates(int MouseX, int MouseY)
        {
        	Matrix4 modelViewMatrix, projectionMatrix;
        	GL.GetFloat(GetPName.ModelviewMatrix, out modelViewMatrix);
        	GL.GetFloat(GetPName.ProjectionMatrix, out projectionMatrix);
        	Matrix4 viewInv = Matrix4.Invert(modelViewMatrix);
        	Matrix4 projInv = Matrix4.Invert(projectionMatrix);
        	Matrix4 M = modelViewMatrix*projectionMatrix;
        	Vector4 vec = new Vector4();
        	vec.X = Zoom*(2*MouseX/(float)ClientRectangle.Width - 1);
        	vec.Y = -Zoom*(2*MouseY/(float)ClientRectangle.Height - 1);
        	vec.Z = M.M43;
        	vec.W = Zoom;
        	Matrix4 MInv = Matrix4.Invert(M);
        	Vector4.Transform(ref vec, ref MInv, out vec);
        	return new Vector2(vec.X, vec.Y);
        }
        
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
        	MouseX = e.X;
        	MouseY = e.Y;
        }
        
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
        	if(e.Button == MouseButton.Left)
        	{ 
        		//LeftMouseDown = true;
        		int playerindex = -1;
        		for(int i = 0; i < Object.ObjectsList.Count; i++)
            	{
            		if(Object.ObjectsList[i].player)
            		{
            			playerindex = i;
            			break;
            		}
				}
        		if(playerindex>=0)
        		{
        			/*GLint viewport = new GLint[4]; //var to hold the viewport info
        			GLdouble modelview = new GLdouble[16]; //var to hold the modelview info
        			GLdouble projection = new GLdouble[16]; //var to hold the projection matrix info
        			GLfloat winX, winY, winZ; //variables to hold screen x,y,z coordinates
        			GLdouble worldX, worldY, worldZ; //variables to hold world x,y,z coordinates
 
        			glGetDoublev( GL_MODELVIEW_MATRIX, modelview ); //get the modelview info
        			glGetDoublev( GL_PROJECTION_MATRIX, projection ); //get the projection matrix info
        			glGetIntegerv( GL_VIEWPORT, viewport ); //get the viewport info
 
					winX = (float)x;
        			winY = (float)viewport[3] - (float)y;
					winZ = 0;
 
					//get the world coordinates from the screen coordinates
        			gluUnProject( winX, winY, winZ, modelview, projection, viewport, worldX, worldY, worldZ);*/
        			//Matrix4 modelview = Matrix4.LookAt(Vector3.UnitZ*Zoom, Vector3.Zero, Vector3.UnitY);
        			//Console.WriteLine("Zoom "+Zoom);
        			//Console.WriteLine("Bottom "+ClientRectangle.Bottom);
        			//Console.WriteLine("MouseY "+e.Y);
        			/*Matrix4 modelViewMatrix, projectionMatrix;
        			GL.GetFloat(GetPName.ModelviewMatrix, out modelViewMatrix);
        			GL.GetFloat(GetPName.ProjectionMatrix, out projectionMatrix);
        			Matrix4 viewInv = Matrix4.Invert(modelViewMatrix);
        			Matrix4 projInv = Matrix4.Invert(projectionMatrix);
        			Vector4 vec;
        			vec.X = e.X;
        			vec.Y = e.Y;
        			vec.Z = 0f;
        			vec.W = 1.0f;*/
        			/*int[] viewport = new int[4];
        			GL.GetInteger(GetPName.Viewport, viewport);
        			System.Drawing.Size vp = new System.Drawing.Size(viewport[2], viewport[3]);
        			Vector4 vec;
        			vec.X = (e.X - (float)vp.Width/2)/(float)vp.Width;//2.0f * e.X / (float)vp.Width - 1;
        			vec.Y = -(2.0f * e.Y / (float)ClientRectangle.Height - 1);
        			vec.Z = 0;
        			vec.W = 1.0f;
        			Vector4.Transform(ref vec, ref projInv, out vec);
        			Vector4.Transform(ref vec, ref viewInv, out vec);
        			if (vec.W > float.Epsilon || vec.W < float.Epsilon)
        			{
            			vec.X /= vec.W;
            			vec.Y /= vec.W;
            			vec.Z /= vec.W;
        			}*/
        			/*vec.X = 10;
        			vec.Y = 0;
        			vec.Z = 0f;
        			vec.W = 1.0f;
        			Console.WriteLine(vec+"\n");
        			Console.WriteLine(modelViewMatrix+"\n");
        			Vector4.Transform(ref vec, ref modelViewMatrix, out vec);
        			Console.WriteLine(vec+"\n");
        			Console.WriteLine(projectionMatrix+"\n");
        			Vector4.Transform(ref vec, ref projectionMatrix, out vec);
        			Console.WriteLine(vec);
        			vec.X = vec.X * 0.5f / vec.W + 0.5f;
					vec.Y = vec.Y * 0.5f / vec.W + 0.5f;
					vec.X *= ClientRectangle.Width;
					vec.Y *= ClientRectangle.Height;*/
        			/*vec.X = (2*vec.X/(float)ClientRectangle.Width - 1)*vec.W;
        			vec.Y = (2*vec.Y/(float)ClientRectangle.Height - 1)*vec.W;
        			Vector4.Transform(ref vec, ref projInv, out vec);
        			Vector4.Transform(ref vec, ref viewInv, out vec);*/
        			
        			/*vec.X = e.X;
        			vec.Y = e.Y;
        			vec.Z = 0f;
        			vec.W = 1.0f;
        			Vector4.Transform(ref vec, ref projInv, out vec);
        			Vector4.Transform(ref vec, ref viewInv, out vec);
        			vec.X = (2*vec.X/(float)ClientRectangle.Width - 1)*vec.W;
        			
        			
        			Console.WriteLine("MouseX "+e.X);
        			Console.WriteLine("MouseY "+e.Y);
        			Console.WriteLine("RezX "+vec.X);
        			Console.WriteLine("RezY "+vec.Y);*/
        			/*Console.WriteLine(vec);
        			Vector4.Transform(ref vec, ref modelViewMatrix, out vec);
        			Vector4.Transform(ref vec, ref projectionMatrix, out vec);
        			Console.WriteLine(vec);
        			Vector4.Transform(ref vec, ref projInv, out vec);
        			Vector4.Transform(ref vec, ref viewInv, out vec);
        			Console.WriteLine(vec);*/
        			/*Console.WriteLine(vec.X);
        			vec.X = vec.X * 0.5f / vec.W + 0.5f;vec.X *= ClientRectangle.Width;
        			Console.WriteLine(vec.X);
        			vec.X = (2*vec.X/(float)ClientRectangle.Width - 1)*vec.W;
        			Console.WriteLine(vec.X);*/
        			/*double S = (2*e.Y-ClientRectangle.Height)*ClientRectangle.Width/((2*e.X-ClientRectangle.Width)*ClientRectangle.Height);
        			double S1 = 2*e.X/ClientRectangle.Width - 1;
        			Matrix4 M = modelViewMatrix*projectionMatrix;
        			double X = (S1*S*M.M21*M.M44-S1*M.M22*M.M44+M.M24*M.M42-S*M.M24*M.M41-S*M.M21*M.M41+M.M22*M.M41-M.M21*M.M42+S*M.M21*M.M41)/(S1*S*M.M14*M.M21-S1*M.M14*M.M22+M.M24*M.M12-S*M.M11*M.M24-S*M.M11*M.M21+M.M11*M.M22-M.M21*M.M12+S*M.M11*M.M21);
        			Console.WriteLine(X);*/
        			/*Matrix4 M = modelViewMatrix*projectionMatrix;
        			Console.WriteLine(vec+"\n");
        			Console.WriteLine(M+"\n");
        			Vector4.Transform(ref vec, ref M, out vec);
        			Console.WriteLine(vec+"\n");*/
        			//Console.WriteLine(modelViewMatrix+"\n");
        			//Console.WriteLine(projectionMatrix+"\n");
        			//Console.WriteLine(projectionMatrix);
        			//Console.WriteLine(vec+"\n");
        			/*Matrix4 M = modelViewMatrix*projectionMatrix;
        			vec.X = Zoom*(2*e.X/(float)ClientRectangle.Width - 1);
        			vec.Y = -Zoom*(2*e.Y/(float)ClientRectangle.Height - 1);
        			vec.Z = M.M43;
        			vec.W = Zoom;
        			Matrix4 MInv = Matrix4.Invert(M);
        			Vector4.Transform(ref vec, ref MInv, out vec);*/
        			Vector2 vec = MouseToWorldCoordinates(e.X, e.Y);
        			//Console.WriteLine(vec.X + " " + vec.Y);
        			double shootx = vec.X - Object.ObjectsList[playerindex].x;//e.X - this.ClientRectangle.Width/2 - Object.ObjectsList[playerindex].x;
        			double shooty = vec.Y - Object.ObjectsList[playerindex].y;//-e.Y + this.ClientRectangle.Height/2 - Object.ObjectsList[playerindex].y;
        			
					double a = Math.Sqrt(shootx*shootx + shooty*shooty);
					if(a <= 0)
					{
						shootx = 1;
						shooty = 1;
					}
					else
					{
						shootx /= a;
						shooty /= a;
					}
					shootx*=Object.ObjectsList[playerindex].ShotSpeed;
					shooty*=Object.ObjectsList[playerindex].ShotSpeed;
					Object.ObjectsList[playerindex].Shoot(shootx, shooty);
        		}
        	}
        }
        
        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
        	if(e.Key == Key.Escape)
        		Exit();
        	int LastPressedKeyNumber = Math.Max(SPressed,WPressed);
			LastPressedKeyNumber = Math.Max(LastPressedKeyNumber, APressed);
			LastPressedKeyNumber = Math.Max(LastPressedKeyNumber, DPressed);
			if(e.Key == Key.S)
				SPressed = LastPressedKeyNumber + 1;
			else
				if(e.Key == Key.W)
					WPressed = LastPressedKeyNumber + 1;
				else
					if(e.Key == Key.A)
						APressed = LastPressedKeyNumber + 1;
					else
						if(e.Key == Key.D)
							DPressed = LastPressedKeyNumber + 1;
        }
        
        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
        	if(e.Key == Key.S)
				SPressed = 0;
			else
				if(e.Key == Key.W)
					WPressed = 0;
				else
					if(e.Key == Key.A)
						APressed = 0;
					else
						if(e.Key == Key.D)
							DPressed = 0;
        }
        
        int FrameCount;
        int LastSecondFrameCount;
        int LastTick;
        
        int FPS()
		{
			if(System.Environment.TickCount - LastTick > 1000)
			{
				LastSecondFrameCount = FrameCount;
				FrameCount = 0;
				LastTick = System.Environment.TickCount;
			}
			FrameCount++;
			return LastSecondFrameCount;
		}
        
        static int LoadTexture(string filename)
    	{
        	if (String.IsNullOrEmpty(filename))
            	throw new ArgumentException(filename);

        	int id = GL.GenTexture();
        	GL.BindTexture(TextureTarget.Texture2D, id);
        
        	System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(filename);
        	System.Drawing.Imaging.BitmapData bmp_data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        	GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
            	OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

        	bmp.UnlockBits(bmp_data);


        	GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        	GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        	GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.MirroredRepeat);
        	GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.MirroredRepeat);

        	return id;
    	}
        public static Game game = null;
        [STAThread]
        static void Main()
        {
            // The 'using' idiom guarantees proper resource cleanup.
            // We request 30 UpdateFrame events per second, and unlimited
            // RenderFrame events (as fast as the computer can handle).
            using (/*Game */game = new Game())
            {
                game.Run(60.0);
            }
        }
    }
}