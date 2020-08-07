import React from 'react';
import { initializeIcons } from '@uifabric/icons';
import { BrowserRouter, Switch, Route, useParams } from "react-router-dom";
import { HeaderBar } from './components';
import { Error404, HomeView, ProjectView } from './view';
import { } from './view/ProjectView';
import './App.css'

interface IAppProps {
  onSignOut: () => void;
  // tenantId: string;
}

interface IAppState {
}

class App extends React.Component<IAppProps, IAppState> {

  constructor(props: IAppProps) {
    super(props);
    initializeIcons();
  }

  render() {
    return (
      <div>
        <BrowserRouter>
          <Switch>
            <Route path="/projects/:projectId">
              <ProjectViewWrapper {...this.props} />
            </Route>
            <Route path="/" exact={true}>
              <HomeViewWrapper {...this.props} />
            </Route>
            <Route path="*">
              <Error404Wrapper {...this.props} />
            </Route>
          </Switch>
        </BrowserRouter>
      </div>
    );
  }
}

function HeaderBarWrapper(props: IAppProps) {
  return <header>
    <HeaderBar onSignOut={props.onSignOut} />
  </header>;
}

function ProjectViewWrapper(props: IAppProps) {
  let { projectId } = useParams();
  return <>
    <HeaderBarWrapper {...props} />
    <ProjectView
      projectId={projectId}
    // tenantId={props.tenantId}
    />
  </>;
}

function HomeViewWrapper(props: IAppProps) {
  // let { subscriptionId } = useParams();
  return <>
    <HeaderBarWrapper {...props} />
    <HomeView />
  </>;
}

function Error404Wrapper(props: IAppProps) {
  return <>
    <HeaderBarWrapper {...props} />
    <Error404 />
  </>;
}

export default App;
