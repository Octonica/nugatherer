package octonica.nugatherer.agent;

import jetbrains.buildServer.RunBuildException;
import jetbrains.buildServer.agent.FlowLogger;
import jetbrains.buildServer.agent.runner.BuildServiceAdapter;
import jetbrains.buildServer.agent.runner.ProgramCommandLine;
import jetbrains.buildServer.messages.BuildMessage1;
import jetbrains.buildServer.messages.Status;
import octonica.nugatherer.common.NuGathererProperties;
import org.jetbrains.annotations.NotNull;
import org.w3c.dom.Document;
import org.w3c.dom.Element;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import javax.xml.transform.OutputKeys;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerException;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.util.Date;
import java.util.Map;

public class NuGathererRunner extends BuildServiceAdapter {

  private static final String MsbuildNs = "http://schemas.microsoft.com/developer/msbuild/2003";
  private static final String TaskName = "NuGathererTask";
  private static final String TaskAssembly = "Octonica.NuGatherer";

  @NotNull
  public ProgramCommandLine makeProgramCommandLine() throws RunBuildException {
    File rootProject = buildRootProject();
    FlowLogger logger=  getFlowLogger();
    logger.logMessage(new BuildMessage1("NuGatherer","INFO", Status.NORMAL, new Date(),"Root project: "+ rootProject.getAbsolutePath()));

    Map<String,String> runnerParameters = getRunnerParameters();
    String version = runnerParameters.getOrDefault(NuGathererProperties.MsbuildVersionProperty, NuGathererProperties.MsbuildVersions.get(0));
    logger.logMessage(new BuildMessage1("NuGatherer","INFO", Status.NORMAL, new Date(), "MSBuild version: " + version));
    String parameter = NuGathererProperties.getMsbuildConfigurationParameter(version);

    Map<String,String> configParameters = getConfigParameters();
    String msbuildPath = configParameters.getOrDefault(parameter,null);
    if(msbuildPath==null){
      msbuildPath = "MSBuild.exe";
    }else {
      msbuildPath = msbuildPath + "/MSBuild.exe";
    }

    return new MsbuildCommandLine(getWorkingDirectory().getAbsolutePath(), msbuildPath, rootProject.getAbsolutePath());
  }

  private File buildRootProject() throws RunBuildException {
    File buildTempFile = this.getBuildTempDirectory();
    buildTempFile = new File(buildTempFile,"NuGatherer.msbuildproj");
    if(buildTempFile.exists()){
      buildTempFile.delete();
    }

    FileOutputStream outputStream = null;
    DocumentBuilderFactory dbf = DocumentBuilderFactory.newInstance();
    try{
      DocumentBuilder db = dbf.newDocumentBuilder();
      Document document = db.newDocument();

      Element root = document.createElementNS(MsbuildNs,"Project");
      Element properties = document.createElement("PropertyGroup");

      Map<String,String> systemProperties = getSystemProperties();
      for(Map.Entry<String, String> entry: systemProperties.entrySet()){
        if(entry.getKey().contains(".")){
          continue;
        }

        Element prop = document.createElement(entry.getKey());
        prop.setTextContent(entry.getValue());
        properties.appendChild(prop);
      }
      root.appendChild(properties);

      Map<String, String> runnerParameters = getRunnerParameters();
      String projectFilePath = runnerParameters.getOrDefault(NuGathererProperties.VirtualProjectRootProperty, null);
      if(projectFilePath == null || projectFilePath.isEmpty()){
        throw new RunBuildException("The virtual project file is not specified.");
      }

      Element importProject = document.createElement("Import");
      importProject.setAttribute("Project", getAbsolutePath(projectFilePath));
      root.appendChild(importProject);

      Element using = document.createElement("UsingTask");
      using.setAttribute("TaskName",TaskAssembly+"."+TaskName);
      File taskPath = new File(NuGathererRunner.class.getProtectionDomain().getCodeSource().getLocation().getPath());
      taskPath = taskPath.getParentFile();
      taskPath = new File(taskPath, TaskAssembly+".dll");
      using.setAttribute("AssemblyFile", taskPath.getAbsolutePath());
      root.appendChild(using);

      Element target = document.createElement("Target");
      target.setAttribute("Name", "Resolve");

      Element task = document.createElement(TaskAssembly+"."+TaskName);
      task.setAttribute("PropertiesFile", "$(MSBuildThisFile)");
      task.setAttribute("RootProjects", "@(ProjectReference)");
      if(runnerParameters.containsKey(NuGathererProperties.OutputFileProperty)){
        String outputFilePath = runnerParameters.get(NuGathererProperties.OutputFileProperty).trim();
        if(!outputFilePath.isEmpty()){
          outputFilePath=getAbsolutePath(outputFilePath);
          task.setAttribute("PackagesOutFile", outputFilePath);
        }
      }

      target.appendChild(task);
      root.appendChild(target);

      String toolsVersion = runnerParameters.getOrDefault(NuGathererProperties.MsbuildToolsVersionProperty,"none");
      if(toolsVersion != null && toolsVersion != "none"){
        root.setAttribute("ToolsVersion", toolsVersion);
      }
      root.setAttribute("DefaultTargets", "Resolve");

      document.appendChild(root);

      Transformer tr = TransformerFactory.newInstance().newTransformer();
      tr.setOutputProperty(OutputKeys.INDENT, "yes");
      tr.setOutputProperty(OutputKeys.METHOD, "xml");
      tr.setOutputProperty(OutputKeys.ENCODING, "UTF-8");
      tr.setOutputProperty("{http://xml.apache.org/xslt}indent-amount", "4");

      outputStream = new FileOutputStream(buildTempFile);
      tr.transform(new DOMSource(document), new StreamResult(outputStream));
    }
    catch (ParserConfigurationException|TransformerException|FileNotFoundException ex){
      throw new RunBuildException(ex.getMessage(), ex);
    }
    finally {
      if(outputStream != null){
        try {
          outputStream.close();
        } catch (IOException e) {
          e.printStackTrace();
        }
      }
    }

    return buildTempFile;
  }

  @NotNull
  private String getAbsolutePath(String path){
    File absolutePath = new File(path);
    if(!absolutePath.isAbsolute()){
      absolutePath = new File(getWorkingDirectory(), path);
    }
    return absolutePath.getAbsolutePath();
  }
}
