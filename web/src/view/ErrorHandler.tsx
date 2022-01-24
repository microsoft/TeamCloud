import React, { Dispatch } from 'react';
// import { useNavigate, useLocation } from 'react-router-dom';
import { useLocation } from 'react-router-dom';
import { Error404 } from '.';

interface IErrorStatusContextProps {
  setErrorStatusCode: Dispatch<number>
}

const ErrorStatusContext = React.createContext({} as IErrorStatusContextProps);

// The top level component that will wrap our app's core features
export const ErrorHandler: React.FC = ({ children }) => {

  const { key } = useLocation();
  // const navigate = useNavigate();
  const [errorStatusCode, setErrorStatusCode] = React.useState(0);

  // Make sure to "remove" this status code whenever the user
  // navigates to a new URL. If we didn't do that, then the user
  // would be "trapped" into error pages forever
  React.useEffect(() => {
    // Listen for changes to the current location.
    // const unlisten = history.listen(() => setErrorStatusCode(0));
    // cleanup the listener on unmount
    return setErrorStatusCode(0);
  }, [key])

  // This is what the component will render. If it has an
  // errorStatusCode that matches an API error, it will only render
  // an error page. If there is no error status, then it will render
  // the children as normal
  const renderContent = () => {
    if (errorStatusCode === 404) {
      return <Error404 />
    }

    // ... more HTTP codes handled here

    return children;
  }

  // We wrap it in a useMemo for performance reasons. More here:
  // https://kentcdodds.com/blog/how-to-optimize-your-context-value/
  const contextPayload = React.useMemo(
    () => ({ setErrorStatusCode } as IErrorStatusContextProps),
    [setErrorStatusCode]
  );

  // We expose the context's value down to our components, while
  // also making sure to render the proper content to the screen
  return (
    <ErrorStatusContext.Provider value={contextPayload}>
      {renderContent()}
    </ErrorStatusContext.Provider>
  )
}

// A custom hook to quickly read the context's value. It's
// only here to allow quick imports
export const useErrorHandler = () => React.useContext(ErrorStatusContext);