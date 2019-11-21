using UnityEngine;

[ExecuteInEditMode]
public class NormalTester : MonoBehaviour {
	
	public bool showNormals = true;
	public Color color = new Color(0,1,0);
	public float length = 0.1f;
	public bool showTangents = false;
	public Color tangentsColor = new Color(0,0,1);
	private Mesh mesh;
	
	// Use this for initialization
	void Start () {
		MeshFilter mf = GetComponent<MeshFilter>();
		if(mf!=null) {
			mesh = mf.sharedMesh;
			
			Debug.Log("Mesh "+mesh.name+ " "+mesh.vertices.Length+" "+mesh.normals.Length+" "+mesh.tangents.Length);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(mesh==null) return;
		
		for(int i=0;i<mesh.vertices.Length;i++) {
			Matrix4x4 mat = transform.localToWorldMatrix;
			Vector3 start = mesh.vertices[i];
			start = mat.MultiplyPoint3x4(start);
			Vector3 end;
			
			if(showNormals && mesh.normals!=null && mesh.normals.Length>0 ) {
				end = mesh.vertices[i] + mesh.normals[i]*length;
				end = mat.MultiplyPoint3x4(end);
				UnityEngine.Debug.DrawLine( start, end, color);
			}
			
			if(showTangents && mesh.tangents!=null && mesh.tangents.Length>0 ) {
				Vector4 t = mesh.tangents[i];
				end = mat.MultiplyPoint3x4( mesh.vertices[i] + (new Vector3(t.x,t.y,t.z)*(t.w*length) ) );
				UnityEngine.Debug.DrawLine( start, end, tangentsColor);
			}
			
		}
	}
}
