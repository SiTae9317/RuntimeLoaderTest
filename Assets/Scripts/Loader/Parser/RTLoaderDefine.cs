using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public partial class RTLoader : IDisposable 
{
	private const string Lights = "\"NodeAttribute::\", \"Light\"";
	private const string Cameras = "\"NodeAttribute::\", \"Camera\"";
	public const string Models = "\"Model::";
	public const string Materials = "\"Material::";
	public const string Textures = ", \"Texture::";
	public const string LayeredTextures = "\"LayeredTexture::";
	public const string Points = "Points: *";
	public const string Vertices = "Vertices: *";
	public const string Deformers = "\"Deformer::";
	public const string Subdeformers = "\"SubDeformer::";
	public const string AnimationStack = "\"AnimStack::";
	public const string AnimationLayer = "\"AnimLayer::";
	public const string AnimationCurveNode = "\"AnimCurveNode::";
	public const string AnimationCurve = "\"AnimCurve::";
	private const int MaximumVertices = 30000;
}
