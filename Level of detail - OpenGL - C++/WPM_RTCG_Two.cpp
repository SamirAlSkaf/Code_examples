#include "pch.h"
#include <iostream>
#include <cmath>
#include <glm.hpp>
#include <gtc/matrix_transform.hpp>
#include <gtc/type_ptr.hpp>
using namespace std;
#include <GL/glew.h>
#include <GLFW/glfw3.h>
#include <time.h>
#include <math.h>
#include <stdlib.h>
#pragma region MethodenVordeklarierung
void createObjects();
void addingObjectShaders();
void compileObjectShaders();
void framebuffer_size_callback(GLFWwindow* window, int width, int height);
void processInput(GLFWwindow *window);
#pragma endregion
#pragma region Variablen
const GLint FENSTERBREITE = 1920, FENSTERHOEHE = 1080;   //Klassisch Fenster  Maße
GLuint VertexArrayObject, ShadersProgram, uniform_projection, obj_ibo;//ShaderProgram für Zusammenfürung  beider Shader,VertexArrayObject für  Vertices mit Attributen
unsigned int indices[]{ 0,3,1,   1,3,2,   2,3,0,   0,1,2 };
#pragma endregion

#pragma region Vertex- und FragmentShader Char*s
//shader werden als char abgespeichert;
//in vec3 pos => Koordinatensystem mit 3Acsen und Variablenname pos
//gl_Position ist der allgemeine Name des vertex, fast wie .this ?
static const char* vertex_shader = R"glsl(
    #version 410 core
    layout (location=0) in vec3 pos;
    out vec4 vertex_colors;
        void main()
        {
          gl_Position =   vec4(pos.x , pos.y, pos.z, 1.0);
          vertex_colors = vec4(1.0, 0.6, 0.0, 1.0);
        }
    )glsl";
//fragmemt shader können mehrer 'out'-variablen/Farben haben
static const char* fragment_shader = R"glsl(
    #version 410 core
    in vec4 vertex_colors;
    out vec4 colors;
        void main()
        {
            colors = vertex_colors;
        }
    )glsl";

static const char* vs_source = R"glsl(
    #version 410 core
        void main(void)
        {
            const vec4 vertices[] =  vec4[](vec4(0.75,-0.75,0.5,1.0), vec4(-0.75,-0.75,0.5,1.0), vec4(0.75, 0.75,0.5,1.0)); 
            gl_Position = vertices[gl_VertexID];
        }
    )glsl";

static const char* tcs_source = R"glsl(
    #version 410 core
    layout (vertices=3) out;    
    void main(void)
    {
            if(gl_InvocationID == 0)
            {
                gl_TessLevelInner[0] = 5.0;
                gl_TessLevelOuter[0] = 5.0;
                gl_TessLevelOuter[1] = 5.0;
                gl_TessLevelOuter[2] = 5.0;
            }
            gl_out[gl_InvocationID].gl_Position = gl_in[gl_InvocationID].gl_Position;
    }
    )glsl";

static const char* tes_source = R"glsl(
     #version 410 core                                                                             
     layout (triangles,equal_spacing,cw) in;                                                                             
     void main(void)                                   
     {                                   
       gl_Position =   (gl_TessCoord.x   *   gl_in[0].gl_Position)   +   (gl_TessCoord.y   *   gl_in[1].gl_Position)  +   (gl_TessCoord.z   *    gl_in[2].gl_Position); 
     }             
   )glsl";

static const char* fs_source = R"glsl(           
     #version 410 core                                                                           
     out vec4 color;                                                                          
     void main(void)                                    
     {                                   
         color = vec4(0.0, 0.8, 1.0, 1.0);                               
     }                                      
   )glsl";

#pragma endregion

int main()
{
    //Überprüfung
    if (!glfwInit()) {
        printf("fehlgeschlagen");
        glfwTerminate();
        return 1;
    }
    #pragma region Kompatibilitätszeug
    glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
    glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
    glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
    glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GL_TRUE);
#pragma endregion
    GLFWwindow * hauptfenster = glfwCreateWindow(FENSTERBREITE, FENSTERHOEHE, "Hauptfenster", NULL, NULL);  //Wieder Fenster erstellen
    if (!hauptfenster) {
        printf("GLFW Fenstererstellung fehlgeschlagen!");
        glfwTerminate();
        return 1;
    }
    //FrameBuffer ist digitales Vorabbild des zukünftigen AnalogBilds am Display
    GLint bufferbreite, bufferlange;                                            //Maße für   Framebuffer als zwei Ints               
    glfwGetFramebufferSize(hauptfenster, &bufferbreite, &bufferlange);         //Framebuffer erstellen
    glfwMakeContextCurrent(hauptfenster);
    glfwSetFramebufferSizeCallback(hauptfenster, framebuffer_size_callback);
    glewExperimental = GL_TRUE;
    if (glewInit() != GLEW_OK) {
        printf("GLEW Initialisierung fehlgeschlagen");
        glfwDestroyWindow(hauptfenster);
        glfwTerminate();
        return -1;
    }
    //glEnable(GL_DEPTH_TEST);
    glViewport(1, 1, bufferbreite, bufferlange);
    createObjects();
    compileObjectShaders();

    while (!glfwWindowShouldClose(hauptfenster)) {
        glfwPollEvents();
        glClearColor(0.3f, 0.3f, 0.3f, 1.0f);           //ClearColor   definieren
        glClear(GL_COLOR_BUFFER_BIT);                   //Ausfürung des   Clears() mit obiger farbe
        glUseProgram(ShadersProgram);                   //Angabe welches   ShaderProgram benutzt werden soll, wir haben hier nur eins
        glBindVertexArray(VertexArrayObject);           //VertexArrayObject   erneut binden
        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, obj_ibo);
        glDrawArrays(GL_TRIANGLES, 0, 3);  
        glDrawElements(GL_LINE_STRIP, 12, GL_UNSIGNED_INT, 0);//Draw(typ,wie   viele acken am anfang überspringen,anzahl der dreiecke)
        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        glBindVertexArray(0);
        glUseProgram(0);
        glfwSwapBuffers(hauptfenster);            //Buffer austauschen
    }
    return 1;
}

void createObjects() {
    GLfloat vertices[]{ 0, 0, 0,    0.25f, 0 , 0,    0, 0.25f, 0 };         //In   vertices[] sind alle Punkte des Objekts aufgelistet

    //VERTEX-DATEN AN GRAFIKKARTE SCHICKEN(Damit es nur einmal passieren muss   und nicht bei jedem neuen Render)
   //VertexBufferObject einrichten
    GLuint  VertexBufferObject;
    glGenBuffers(1, &VertexBufferObject);       //Buffer der Daten wird   generiert
    glBindBuffer(GL_ARRAY_BUFFER, VertexBufferObject); //Macht VertexObject zum   aktiven ArrayBuffer für die Daten; Um als nächstes die Daten da rein zu laden
    glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);    //vertices-Daten in VertexBufferObject laden
    //glBufferData(Aktiver VertexDatenBuffer,Größe,name,Lade&Draw-Verhalten);

    glGenBuffers(1, &obj_ibo);
    glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, obj_ibo);
    glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(indices), indices,
        GL_STATIC_DRAW);

    glGenVertexArrays(1, &VertexArrayObject);
    glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, obj_ibo);//VertexArrayObject   generieren
    glBindVertexArray(VertexArrayObject);               //Zum benutzen noch   einbinden
    //Link zwischen Shader-Variablen und vertex-Daten
    glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 0, 0);
    //glVerAttPoi(input,anzahl der Dimensionen Vec3,typ der Var,normalisiert   oder nicht,stride: anzahl der bytes zwischen den Punkten?,offset: wie viele   bytes nach dem start kommt die variable)
    glEnableVertexAttribArray(0); //Als Letzes Attribute aktivieren
    //glBindBuffer(GL_ARRAY_BUFFER, 0);
    glBindVertexArray(0);
}

void addingObjectShaders(GLuint shader_program, const char*shader_Code, GLenum   shader_type) {
    //SHADER ERSTELLEN UND UNSEREN SHADER(CHAR*) ALS SOURCE EINBINDEN
    GLuint Shader = glCreateShader(shader_type); //Shader wird erstellt
    const GLchar* ShaderChar[1];                    //Unser ShaderCode verwenbar   machen(Variablen rausholen für Definition der ShaderSource)
    ShaderChar[0] = shader_Code;
    GLint ShaderCharLength[1];
    ShaderCharLength[0] = strlen(shader_Code);
    glShaderSource(Shader, 1, ShaderChar, ShaderCharLength);      //Definition   Shader-Source
    glCompileShader(Shader);      // Shader compilieren lassen
    #pragma region Überprüfung
        GLint status = 0;                   //Zwei Variablen mit denen überprüft   werden kann ob alles erfolgreich war und die Log-infos dazu
        GLchar compileLog[1024] = { 0 };
        glGetShaderiv(Shader, GL_COMPILE_STATUS, &status);      //ÜBERPRÜFUNG:   Bedeutet: Check von Shader den GL_Compile-Status und mach status = Compilestatus
        if (!status) {                                                //Wenn nicht   erfolgreich:
            glGetShaderInfoLog(Shader, sizeof(compileLog), NULL, compileLog);       //getInfoLog vom Shader und gib compileLog die Werte
            printf("Fehler bei der Kompilierung des '%d' Shaders: '%s' \n", shader_type, compileLog);
            return;
        }
    #pragma endregion
    glAttachShader(shader_program, Shader); //Als letztes den Shader dem   übergeordneten Shader-Program zuweisen
    return;
}

void compileObjectShaders() {
    ShadersProgram = glCreateProgram();     //Gemeinsames Shader-Program für   beide Shader(vertex und shader)
     //Überprüfung ob erfolgreich
    if (!ShadersProgram) {
        printf("Erstellung des Shader-Programms fehlgeschlagen!");
        return;
    }
    //Shader-Function wird für beide Shader aufgerufen
    //addingObjectShaders(ShadersProgram, vertex_shader, GL_VERTEX_SHADER);
    //addingObjectShaders(ShadersProgram, fragment_shader, GL_FRAGMENT_SHADER);
    addingObjectShaders(ShadersProgram, vs_source, GL_VERTEX_SHADER);
    addingObjectShaders(ShadersProgram, tcs_source, GL_TESS_CONTROL_SHADER);
    addingObjectShaders(ShadersProgram, tes_source, GL_TESS_EVALUATION_SHADER);
    addingObjectShaders(ShadersProgram, fs_source, GL_FRAGMENT_SHADER);
    glLinkProgram(ShadersProgram);      //Shader-Program aktivieren
    #pragma region Überprüfungen
    GLint status = 0;
    GLchar compileLog[1024] = { 0 };
    glGetProgramiv(ShadersProgram, GL_LINK_STATUS, &status);
    if (!status) {
        glGetProgramInfoLog(ShadersProgram, sizeof(compileLog), NULL, compileLog);
        printf("Fehler beim Linking des Shader Programms: '%s' \n", compileLog);
        return;
    }
    glValidateProgram(ShadersProgram);
    glGetProgramiv(ShadersProgram, GL_VALIDATE_STATUS, &status);
    if (!status) {
        glGetProgramInfoLog(ShadersProgram, sizeof(compileLog), NULL, compileLog);
        printf("Fehler bei der Validierung des Shader Programms: '%s'\n", compileLog);
        return;
    }
#pragma endregion
}

void framebuffer_size_callback(GLFWwindow* window, int width, int heigth)
{
    glViewport(1, 1, width, heigth);
}