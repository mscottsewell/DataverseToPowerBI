/**
 * TabNavigation - Horizontal tab bar for main app sections
 */

import {
  makeStyles,
  Tab,
  TabList,
  tokens,
} from '@fluentui/react-components';
import {
  Settings24Regular,
  Table24Regular,
  Organization24Regular,
  TextColumnOne24Regular,
  BuildingFactory24Regular,
} from '@fluentui/react-icons';
import { useUIStore, type AppTab } from '../../stores';

const useStyles = makeStyles({
  nav: {
    borderBottom: `1px solid ${tokens.colorNeutralStroke2}`,
    backgroundColor: tokens.colorNeutralBackground1,
    paddingLeft: '24px',
    flexShrink: 0,
  },
});

const tabs: { value: AppTab; label: string; icon: JSX.Element }[] = [
  { value: 'setup', label: 'Setup', icon: <Settings24Regular /> },
  { value: 'tables', label: 'Tables', icon: <Table24Regular /> },
  { value: 'schema', label: 'Star Schema', icon: <Organization24Regular /> },
  { value: 'attributes', label: 'Attributes', icon: <TextColumnOne24Regular /> },
  { value: 'build', label: 'Build', icon: <BuildingFactory24Regular /> },
];

export function TabNavigation() {
  const styles = useStyles();
  const activeTab = useUIStore((s) => s.activeTab);
  const setActiveTab = useUIStore((s) => s.setActiveTab);

  return (
    <nav className={styles.nav}>
      <TabList
        selectedValue={activeTab}
        onTabSelect={(_, data) => setActiveTab(data.value as AppTab)}
        size="medium"
      >
        {tabs.map((tab) => (
          <Tab key={tab.value} value={tab.value} icon={tab.icon}>
            {tab.label}
          </Tab>
        ))}
      </TabList>
    </nav>
  );
}
